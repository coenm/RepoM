namespace RepoM.Api.IO.ModuleBasedRepositoryActionProvider;

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Text;
using DotNetEnv;
using Microsoft.Extensions.Logging;
using RepoM.Api.Common;
using RepoM.Api.Git;
using RepoM.Api.IO.ModuleBasedRepositoryActionProvider.ActionMappers;
using RepoM.Api.IO.ModuleBasedRepositoryActionProvider.Data;
using RepoM.Api.IO.ModuleBasedRepositoryActionProvider.Deserialization;
using RepoM.Api.IO.ModuleBasedRepositoryActionProvider.Exceptions;
using RepoM.Api.IO.Variables;
using RepoM.Core.Plugin.Common;
using RepoM.Core.Plugin.Expressions;
using RepoM.Core.Plugin.RepositoryActions.Actions;
using IRepository = RepoM.Core.Plugin.Repository.IRepository;
using Repository = RepoM.Api.Git.Repository;
using RepositoryAction = RepoM.Api.Git.RepositoryAction;

public class RepositoryConfigurationReader
{
    private readonly IAppDataPathProvider _appDataPathProvider;
    private readonly IFileSystem _fileSystem;
    private readonly JsonDynamicRepositoryActionDeserializer _jsonAppSettingsDeserializer;
    private readonly YamlDynamicRepositoryActionDeserializer _yamlAppSettingsDeserializer;
    private readonly IRepositoryExpressionEvaluator _repoExpressionEvaluator;
    private readonly ILogger _logger;

    private const string FILENAME = "RepositoryActions.";
    public const string FILENAME_JSON = FILENAME + "json";

    public RepositoryConfigurationReader(
        IAppDataPathProvider appDataPathProvider,
        IFileSystem fileSystem,
        JsonDynamicRepositoryActionDeserializer jsonAppSettingsDeserializer,
        YamlDynamicRepositoryActionDeserializer yamlAppSettingsDeserializer,
        IRepositoryExpressionEvaluator repoExpressionEvaluator,
        ILogger logger)
    {
        _appDataPathProvider = appDataPathProvider ?? throw new ArgumentNullException(nameof(appDataPathProvider));
        _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
        _jsonAppSettingsDeserializer = jsonAppSettingsDeserializer ?? throw new ArgumentNullException(nameof(jsonAppSettingsDeserializer));
        _yamlAppSettingsDeserializer = yamlAppSettingsDeserializer ?? throw new ArgumentNullException(nameof(yamlAppSettingsDeserializer));
        _repoExpressionEvaluator = repoExpressionEvaluator ?? throw new ArgumentNullException(nameof(repoExpressionEvaluator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    private string GetRepositoryActionsFilename(string basePath)
    {
        var extensions = new [] { "yml", "yaml", "json", };

        var path = Path.Combine(basePath, FILENAME);
        foreach (var ext in extensions)
        {
            var filename = path + ext;
            if (_fileSystem.File.Exists(filename))
            {
                return filename;
            }
        }

        var failingFilename = path + "{" +  string.Join(",",extensions) + "}";
        throw new ConfigurationFileNotFoundException(failingFilename);
    }

    public (Dictionary<string, string>? envVars, List<EvaluatedVariable>? Variables, List<ActionsCollection>? actions, List<TagsCollection>? tags) Get(params IRepository[] repositories)
    {
        if (!repositories.Any())
        {
            return (null, null, null, null);
        }

        IRepository? repository = repositories.FirstOrDefault(); //todo
        if (repository == null)
        {
            return (null, null, null, null);
        }

        var multipleRepositoriesSelected = repositories.Length > 1;

        var variables = new List<EvaluatedVariable>();
        Dictionary<string, string>? envVars = null;
        var actions = new List<ActionsCollection>();
        var tags = new List<TagsCollection>();

        // load default file
        RepositoryActionConfiguration? rootFile = null;
        RepositoryActionConfiguration? repoSpecificConfig = null;

        var filename = GetRepositoryActionsFilename(_appDataPathProvider.GetAppDataPath());
        if (!_fileSystem.File.Exists(filename))
        {
            throw new ConfigurationFileNotFoundException(filename);
        }

        try
        {
            var content = _fileSystem.File.ReadAllText(filename, Encoding.UTF8);
            rootFile = Deserialize(Path.GetExtension(filename), content);
        }
        catch (Exception e)
        {
            throw new InvalidConfigurationException(filename, e.Message, e);
        }
        
        Redirect? redirect = rootFile.Redirect;
        if (!string.IsNullOrWhiteSpace(redirect?.Filename))
        {
            if (IsEnabled(redirect.Enabled, true, null))
            {
                filename = EvaluateString(redirect.Filename, null);
                if (_fileSystem.File.Exists(filename))
                {
                    try
                    {
                        var content = _fileSystem.File.ReadAllText(filename, Encoding.UTF8);
                        rootFile = Deserialize(Path.GetExtension(filename), content);
                    }
                    catch (Exception e)
                    {
                        _logger.LogWarning(e, "Could not read and deserialize file '{file}'", filename);
                        throw new InvalidConfigurationException(filename, e.Message, e);
                    }
                }
            }
        }

        List<EvaluatedVariable> EvaluateVariables(IEnumerable<Variable>? vars)
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

        List<EvaluatedVariable> list = EvaluateVariables(rootFile.Variables);
        variables.AddRange(list);
        using IDisposable rootVariables = RepoMVariableProviderStore.Push(list);

        if (!multipleRepositoriesSelected)
        {
            // load repo specific environment variables
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
                    IEnumerable<KeyValuePair<string, string>>? currentEnvVars = Env.Load(f, new LoadOptions(setEnvVars: false));
                    if (envVars == null || !envVars.Any())
                    {
                        envVars = currentEnvVars.ToDictionary();
                        continue;
                    }

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
        }

        using IDisposable repoSpecificEnvVariables = EnvironmentVariableStore.Set(envVars ?? new Dictionary<string, string>(0));

        if (!multipleRepositoriesSelected)
        {
            // load repo specific config
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

                // todo redirect

                try
                {
                    var content = _fileSystem.File.ReadAllText(f, Encoding.UTF8);
                    repoSpecificConfig = Deserialize(Path.GetExtension(f), content);
                }
                catch (Exception)
                {
                    // warning.
                }
            }
        }
        
        List<EvaluatedVariable> list2 = EvaluateVariables(repoSpecificConfig?.Variables);
        variables.AddRange(list2);
        using IDisposable repoSpecificVariables = RepoMVariableProviderStore.Push(list2);

        actions.Add(rootFile.ActionsCollection);
        if (repoSpecificConfig?.ActionsCollection != null)
        {
            actions.Add(repoSpecificConfig.ActionsCollection);
        }

        tags.Add(rootFile.TagsCollection);
        if (repoSpecificConfig?.TagsCollection != null)
        {
            tags.Add(repoSpecificConfig.TagsCollection);
        }

        return (envVars, variables, actions, tags);
    }

    private object? Evaluate(object? input, IRepository? repository)
    {
        if (input is not string s)
        {
            return input;
        }

        IRepository[] repositories = repository == null ? Array.Empty<IRepository>() : new[] { repository, };
        return _repoExpressionEvaluator.EvaluateValueExpression(s, repositories);

    }

    private string EvaluateString(string? input, IRepository? repository)
    {
        object? v = Evaluate(input, repository);
        return v?.ToString() ?? string.Empty;
    }

    private bool IsEnabled(string? booleanExpression, bool defaultWhenNullOrEmpty, IRepository? repository)
    {
        return string.IsNullOrWhiteSpace(booleanExpression)
            ? defaultWhenNullOrEmpty
            : _repoExpressionEvaluator.EvaluateBooleanExpression(booleanExpression!, repository);
    }

    private RepositoryActionConfiguration Deserialize(string extension, string rawContent)
    {
        if (extension.StartsWith('.'))
        {
            extension = extension[1..];
        }

        if ("json".Equals(extension, StringComparison.CurrentCultureIgnoreCase))
        {
            return _jsonAppSettingsDeserializer.Deserialize(rawContent);
        }

        if ("yaml".Equals(extension, StringComparison.CurrentCultureIgnoreCase) || "yml".Equals(extension, StringComparison.CurrentCultureIgnoreCase))
        {
            return _yamlAppSettingsDeserializer.Deserialize(rawContent);
        }

        throw new NotImplementedException("Unknown extension");
    }
}

public class RepositoryTagsConfigurationFactory : IRepositoryTagsFactory 
{
    private readonly IRepositoryExpressionEvaluator _repoExpressionEvaluator;
    private readonly RepositoryConfigurationReader _repoConfigReader;

    public RepositoryTagsConfigurationFactory(
        IRepositoryExpressionEvaluator repoExpressionEvaluator,
        RepositoryConfigurationReader repoConfigReader)
    {
        _repoExpressionEvaluator = repoExpressionEvaluator ?? throw new ArgumentNullException(nameof(repoExpressionEvaluator));
        _repoConfigReader = repoConfigReader ?? throw new ArgumentNullException(nameof(repoConfigReader));
    }

    public IEnumerable<string> GetTags(Repository repository)
    {
        return GetTagsInner(repository).Distinct();
    }

    private IEnumerable<string> GetTagsInner(IRepository repository)
    {
        List<EvaluatedVariable> EvaluateVariables(IEnumerable<Variable>? vars)
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

        Dictionary<string, string>? repositoryEnvVars;
        List<EvaluatedVariable>? variables;
        List<TagsCollection>? tags;

        try
        {
            (repositoryEnvVars,  variables, _,  tags) = _repoConfigReader.Get(repository);
        }
        catch (Exception)
        {
             // todo, log
             yield break;
        }

        using IDisposable d1 = RepoMVariableProviderStore.Push(variables ?? new List<EvaluatedVariable>(0));
        using IDisposable d2 = EnvironmentVariableStore.Set(repositoryEnvVars);

        foreach (TagsCollection tagsCollection in tags?.Where(t => t != null) ?? Array.Empty<TagsCollection>())
        {
            using IDisposable d3 = RepoMVariableProviderStore.Push(EvaluateVariables(tagsCollection.Variables));

            foreach (RepositoryActionTag action in tagsCollection.Tags)
            {
                if (!IsEnabled(action.When, true, repository))
                {
                    continue;
                }

                if (!string.IsNullOrWhiteSpace(action.Tag))
                {
                    yield return action.Tag!;
                }
            }
        }
    }

    private object? Evaluate(object? input, IRepository repository)
    {
        if (input is string s)
        {
            return _repoExpressionEvaluator.EvaluateValueExpression(s, repository);
        }

        return input;
    }

    private bool IsEnabled(string? booleanExpression, bool defaultWhenNullOrEmpty, IRepository repository)
    {
        return string.IsNullOrWhiteSpace(booleanExpression)
            ? defaultWhenNullOrEmpty
            : _repoExpressionEvaluator.EvaluateBooleanExpression(booleanExpression, repository);
    }
}

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

    public IEnumerable<RepositoryActionBase> CreateActions(params Repository[] repositories)
    {
        if (repositories == null)
        {
            throw new ArgumentNullException(nameof(repositories));
        }

        Repository? singleRepository = null;
        var multiSelectRequired = repositories.Length > 1;
        if (!multiSelectRequired)
        {
            singleRepository = repositories.FirstOrDefault();
        }

        Dictionary<string, string>? repositoryEnvVars = null;
        List<EvaluatedVariable>? variables = null;
        List<ActionsCollection>? actions = null;
        Exception? ex = null;
        try
        {
            (repositoryEnvVars,  variables, actions, _) = _repoConfigReader.Get(repositories);
        }
        catch (Exception e)
        {
            ex = e;
        }

        if (ex != null)
        {
            if (ex is ConfigurationFileNotFoundException configurationFileNotFoundException)
            {
                foreach (RepositoryAction failingItem in CreateFailing(configurationFileNotFoundException, configurationFileNotFoundException.Filename, singleRepository!)) // todo coenm
                {
                    yield return failingItem;
                }
            }
            else
            {
                foreach (RepositoryAction failingItem in CreateFailing(ex, null, singleRepository!)) // todo coenm
                {
                    yield return failingItem;
                }
            }

            yield break;
        }

        using IDisposable d1 = RepoMVariableProviderStore.Push(variables ?? new List<EvaluatedVariable>(0));
        using IDisposable d2 = EnvironmentVariableStore.Set(repositoryEnvVars ?? new Dictionary<string, string>());

        // load variables global
        foreach (ActionsCollection actionsCollection in actions?.Where(action => action != null) ?? Array.Empty<ActionsCollection>())
        {
            using IDisposable d3 = RepoMVariableProviderStore.Push(EvaluateVariables(actionsCollection.Variables, singleRepository));

            foreach (Data.RepositoryAction action in actionsCollection.Actions)
            {
                using IDisposable d4 = RepoMVariableProviderStore.Push(EvaluateVariables(action.Variables, singleRepository));

                if (multiSelectRequired)
                {
                    var actionNotCapableForMultipleRepos = repositories.Any(repo => !IsEnabled(action.MultiSelectEnabled, false, repo));
                    if (actionNotCapableForMultipleRepos)
                    {
                        continue;
                    }
                }

                IEnumerable<RepositoryActionBase> result = _actionMapper.Map(action, repositories);

                foreach (RepositoryActionBase singleItem in result)
                {
                    yield return singleItem;
                }
            }
        }
    }

    private List<EvaluatedVariable> EvaluateVariables(IEnumerable<Variable>? vars, Repository? repository)
    {
        if (vars == null || repository == null)
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
                    Action = new DelegateAction((_, _) =>
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

    private object? Evaluate(object? input, Repository repository)
    {
        if (input is string s)
        {
            return _repoExpressionEvaluator.EvaluateValueExpression(s, repository);
        }

        return input;
    }

    private bool IsEnabled(string? booleanExpression, bool defaultWhenNullOrEmpty, Repository repository)
    {
        return string.IsNullOrWhiteSpace(booleanExpression)
            ? defaultWhenNullOrEmpty
            : _repoExpressionEvaluator.EvaluateBooleanExpression(booleanExpression!, repository);
    }
}