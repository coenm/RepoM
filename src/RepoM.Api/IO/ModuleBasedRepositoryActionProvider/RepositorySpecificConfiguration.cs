namespace RepoM.Api.IO.ModuleBasedRepositoryActionProvider;

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Runtime.Caching;
using Microsoft.Extensions.Logging;
using RepoM.Api.Common;
using RepoM.Api.IO.ModuleBasedRepositoryActionProvider.ActionMappers;
using RepoM.Api.IO.ModuleBasedRepositoryActionProvider.Data;
using RepoM.Api.IO.ModuleBasedRepositoryActionProvider.Deserialization;
using RepoM.Api.IO.ModuleBasedRepositoryActionProvider.Exceptions;
using RepoM.Api.IO.ModuleBasedRepositoryActionProvider.FileCache;
using RepoM.Api.RepositoryActions;
using RepoM.Core.Plugin.Common;
using RepoM.Core.Plugin.Expressions;
using RepoM.Core.Plugin.RepositoryActions.Commands;
using IRepository = RepoM.Core.Plugin.Repository.IRepository;
using Repository = RepoM.Api.Git.Repository;
using RepositoryAction = RepoM.Api.RepositoryActions.RepositoryAction;

public class RepositoryConfigurationReader
{
    public const string FILENAME = "RepositoryActions.yaml";
    private readonly IAppDataPathProvider _appDataPathProvider;
    private readonly IFileSystem _fileSystem;
    private readonly IRepositoryExpressionEvaluator _repoExpressionEvaluator;
    private readonly ILogger _logger;
    private readonly RepositoryActionsFileStore _repositoryActionsFileStore;
    private readonly EnvFileStore _envFileStore;

    public RepositoryConfigurationReader(
        IAppDataPathProvider appDataPathProvider,
        IFileSystem fileSystem,
        IRepositoryActionDeserializer repositoryActionsDeserializer,
        IRepositoryExpressionEvaluator repoExpressionEvaluator,
        ILogger logger,
        ObjectCache cache)
    {
        _appDataPathProvider = appDataPathProvider ?? throw new ArgumentNullException(nameof(appDataPathProvider));
        _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
        _repoExpressionEvaluator = repoExpressionEvaluator ?? throw new ArgumentNullException(nameof(repoExpressionEvaluator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _repositoryActionsFileStore = new RepositoryActionsFileStore(_fileSystem, repositoryActionsDeserializer, cache);
        _envFileStore = new EnvFileStore(cache);
    }

    private string GetRepositoryActionsFilename(string basePath)
    {
        var filename = Path.Combine(basePath, FILENAME);
        if (_fileSystem.File.Exists(filename))
        {
            return filename;
        }

        var filenameTemplate = Path.Combine(_appDataPathProvider.AppResourcesPath, FILENAME);
        if (_fileSystem.File.Exists(filenameTemplate))
        {
            try
            {
                _fileSystem.File.Copy(filenameTemplate, filename);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Could not copy default {FILENAME} to {AppDataPath}", FILENAME, _appDataPathProvider.AppDataPath);
            }

            if (_fileSystem.File.Exists(filename))
            {
                return filename;
            }
        }

        throw new ConfigurationFileNotFoundException(filename);
    }

    public (Dictionary<string, string>? envVars, List<EvaluatedVariable>? Variables, List<ActionsCollection>? actions) Get(IRepository repository)
    {
        if (repository == null)
        {
            return (null, null, null);
        }

        var variables = new List<EvaluatedVariable>();
        var actions = new List<ActionsCollection>();

        // load default file
        RepositoryActionConfiguration? rootFile;
        
        var filename = GetRepositoryActionsFilename(_appDataPathProvider.AppDataPath);
        if (!_fileSystem.File.Exists(filename))
        {
            throw new ConfigurationFileNotFoundException(filename);
        }

        try
        {
            rootFile = _repositoryActionsFileStore.TryGet(filename);
        }
        catch (Exception e)
        {
            throw new InvalidConfigurationException(filename, e.Message, e);
        }

        if (rootFile == null)
        {
            throw new InvalidConfigurationException(filename, "Could not read and deserialize file");
        }

        rootFile = RedirectRootFile(rootFile);

        List<EvaluatedVariable> list = EvaluateVariables(rootFile.Variables, repository);
        variables.AddRange(list);

        // Load repo specific environment variables
        Dictionary<string, string> envVars = LoadRepoEnvironmentVariables(repository, rootFile);

        // Load repo specific config
        RepositoryActionConfiguration? repoSpecificConfig = LoadRepoSpecificConfig(repository, rootFile);
        List<EvaluatedVariable> repoSpecificVariables = EvaluateVariables(repoSpecificConfig?.Variables, repository);
        variables.AddRange(repoSpecificVariables);

        actions.Add(rootFile.ActionsCollection);
        if (repoSpecificConfig?.ActionsCollection != null)
        {
            actions.Add(repoSpecificConfig.ActionsCollection);
        }

        return (envVars, variables, actions);
    }

    private RepositoryActionConfiguration? LoadRepoSpecificConfig(IRepository repository, RepositoryActionConfiguration rootFile)
    {
        RepositoryActionConfiguration? repoSpecificConfig = null;

        foreach (FileReference fileRef in rootFile.RepositorySpecificConfigFiles)
        {
            if (repoSpecificConfig != null)
            {
                continue;
            }

            if (fileRef == null || !IsEnabled(fileRef.When, true, repository))
            {
                continue;
            }

            var f = EvaluateString(fileRef.Filename, repository);
            if (!_fileSystem.File.Exists(f))
            {
                // warning
                continue;
            }

            try
            {
                repoSpecificConfig = _repositoryActionsFileStore.TryGet(f);
            }
            catch (Exception)
            {
                // warning.
            }
        }

        return repoSpecificConfig;
    }

    private Dictionary<string, string> LoadRepoEnvironmentVariables(IRepository repository, RepositoryActionConfiguration rootFile)
    {
        var envVars = new Dictionary<string, string>();

        foreach (FileReference fileRef in rootFile.RepositorySpecificEnvironmentFiles.Where(fileRef => fileRef != null))
        {
            if (!IsEnabled(fileRef.When, true, repository))
            {
                continue;
            }

            var f = EvaluateString(fileRef.Filename, repository);
            if (!_fileSystem.File.Exists(f))
            {
                // log warning?
                continue;
            }

            try
            {
                IDictionary<string, string> currentEnvVars = _envFileStore.TryGet(f);

                foreach (KeyValuePair<string, string> item in currentEnvVars)
                {
                    if (!envVars.ContainsKey(item.Key))
                    {
                        envVars.Add(item.Key, item.Value);
                    }
                    else
                    {
                        _logger.LogTrace("Environment key was '{Key}' already set.", item.Key);
                    }
                }
            }
            catch (Exception e)
            {
                _logger.LogWarning(e, "Something went wrong loading an environment file");
            }
        }

        return envVars;
    }

    private List<EvaluatedVariable> EvaluateVariables(IEnumerable<Variable>? vars, IRepository repository)
    {
        if (vars == null)
        {
            return new List<EvaluatedVariable>(0);
        }

        return vars
               .Where(v => IsEnabled(v.Enabled, true, repository))
               .Select(v => new EvaluatedVariable
                   {
                       Name = v.Name,
                       Value = Evaluate(v.Value, repository),
                   })
               .ToList();
    }

    private RepositoryActionConfiguration RedirectRootFile(RepositoryActionConfiguration rootFile)
    {
        Redirect? redirect = rootFile.Redirect;

        if (string.IsNullOrWhiteSpace(redirect?.Filename) || !IsEnabled(redirect.Enabled, true, null))
        {
            return rootFile;
        }

        var filename = EvaluateString(redirect.Filename, null);
        if (!_fileSystem.File.Exists(filename))
        {
            return rootFile;
        }

        RepositoryActionConfiguration? result;

        try
        {
            result = _repositoryActionsFileStore.TryGet(filename);
        }
        catch (Exception e)
        {
            _logger.LogWarning(e, "Could not read and deserialize file '{file}'", filename);
            throw new InvalidConfigurationException(filename, e.Message, e);
        }

        if (result == null)
        {
            throw new InvalidConfigurationException(filename, "Could not read and deserialize file");
        }

        return result;
    }

    private object? Evaluate(object? input, IRepository? repository)
    {
        if (input is not string s)
        {
            return input;
        }

        return _repoExpressionEvaluator.EvaluateValueExpression(s, repository);
    }

    private string EvaluateString(string? input, IRepository? repository)
    {
        var v = Evaluate(input, repository);
        return v?.ToString() ?? string.Empty;
    }

    private bool IsEnabled(string? booleanExpression, bool defaultWhenNullOrEmpty, IRepository? repository)
    {
        return string.IsNullOrWhiteSpace(booleanExpression)
            ? defaultWhenNullOrEmpty
            : _repoExpressionEvaluator.EvaluateBooleanExpression(booleanExpression, repository);
    }
}

[Obsolete("Old style menu")]
public class RepositorySpecificConfiguration
{
    private readonly IFileSystem _fileSystem;
    private readonly IRepositoryExpressionEvaluator _repoExpressionEvaluator;
    private readonly ActionMapperComposition _actionMapper;
    private readonly ITranslationService _translationService;
    private readonly RepositoryConfigurationReader _repoConfigReader;

    public RepositorySpecificConfiguration(
        IFileSystem fileSystem,
        IRepositoryExpressionEvaluator repoExpressionEvaluator,
        ActionMapperComposition actionMapper,
        ITranslationService translationService,
        RepositoryConfigurationReader repoConfigReader)
    {
        _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
        _repoExpressionEvaluator = repoExpressionEvaluator ?? throw new ArgumentNullException(nameof(repoExpressionEvaluator));
        _actionMapper = actionMapper ?? throw new ArgumentNullException(nameof(actionMapper));
        _translationService = translationService ?? throw new ArgumentNullException(nameof(translationService));
        _repoConfigReader = repoConfigReader ?? throw new ArgumentNullException(nameof(repoConfigReader));
    }

    public IEnumerable<RepositoryActionBase> CreateActions(Repository repository)
    {
        Dictionary<string, string>? repositoryEnvVars = null;
        List<EvaluatedVariable>? variables = null;
        List<ActionsCollection>? actions = null;
        Exception? ex = null;
        try
        {
            (repositoryEnvVars,  variables, actions) = _repoConfigReader.Get(repository);
        }
        catch (Exception e)
        {
            ex = e;
        }

        if (ex != null)
        {
            if (ex is ConfigurationFileNotFoundException configurationFileNotFoundException)
            {
                foreach (RepositoryAction failingItem in CreateFailing(configurationFileNotFoundException, configurationFileNotFoundException.Filename, repository))
                {
                    yield return failingItem;
                }
            }
            else
            {
                foreach (RepositoryAction failingItem in CreateFailing(ex, null, repository))
                {
                    yield return failingItem;
                }
            }

            yield break;
        }

        // load variables global
        IEnumerable<ActionsCollection> iterateOverActions = actions != null ? actions : Array.Empty<ActionsCollection>();

        foreach (ActionsCollection actionsCollection in iterateOverActions)
        {
            foreach (Data.RepositoryAction action in actionsCollection.Actions)
            {
                IEnumerable<RepositoryActionBase> result = _actionMapper.Map(action, repository);

                foreach (RepositoryActionBase singleItem in result)
                {
                    yield return singleItem;
                }
            }
        }
    }

    private IEnumerable<RepositoryAction> CreateFailing(Exception ex, string? filename, IRepository repository)
    {
        yield return new RepositoryAction(_translationService.Translate("Could not read repository actions"), repository)
            {
                CanExecute = false,
            };

        yield return new RepositoryAction(ex.Message, repository)
            {
                CanExecute = false,
            };

        if (!string.IsNullOrWhiteSpace(filename))
        {
            yield return new RepositoryAction(_translationService.Translate("Fix"), repository)
                {
                    Action = new DelegateRepositoryCommand((_, _) =>
                        {
                            var directoryName = _fileSystem.Path.GetDirectoryName(filename);
                            if (directoryName != null)
                            {
                                ProcessHelper.StartProcess(directoryName, string.Empty);
                            }
                        }),
                };
        }
    }
}