namespace RepoM.ActionMenu.Core.Services;

using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RepoM.ActionMenu.Core;
using RepoM.ActionMenu.Core.ConfigReader;
using RepoM.ActionMenu.Core.Misc;
using RepoM.ActionMenu.Core.Model;
using RepoM.ActionMenu.Core.Yaml.Model;
using RepoM.ActionMenu.Core.Yaml.Model.ActionContext.EvaluateVariable;
using RepoM.ActionMenu.Core.Yaml.Model.ActionContext.ExecuteScript;
using RepoM.ActionMenu.Core.Yaml.Model.ActionContext.LoadFile;
using RepoM.ActionMenu.Core.Yaml.Model.ActionContext.RendererVariable;
using RepoM.ActionMenu.Core.Yaml.Model.ActionContext.SetVariable;
using RepoM.ActionMenu.Core.Yaml.Model.ActionContext;
using RepoM.ActionMenu.Interface.Scriban;
using RepoM.ActionMenu.Interface.UserInterface;
using RepoM.ActionMenu.Interface.YamlModel;
using RepoM.Core.Plugin.Repository;

internal class UserInterfaceActionMenuFactory : IUserInterfaceActionMenuFactory
{
    private readonly IFileSystem _fileSystem;
    private readonly ITemplateParser _templateParser;
    private readonly ITemplateContextRegistration[] _plugins;
    private readonly IActionToRepositoryActionMapper[] _mappers;
    private readonly IActionMenuDeserializer _deserializer;
    private readonly IFileReader _fileReader;
    private readonly ILogger _logger;
    private readonly IContextActionProcessor[] _contextActionMappers;

    public UserInterfaceActionMenuFactory(
        IFileSystem fileSystem, 
        ITemplateParser templateParser,
        IEnumerable<ITemplateContextRegistration> plugins,
        IEnumerable<IActionToRepositoryActionMapper> mappers,
        IActionMenuDeserializer deserializer,
        IFileReader fileReader,
        ILogger logger)
    {
        _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
        _templateParser = templateParser ?? throw new ArgumentNullException(nameof(templateParser));
        _plugins = plugins.ToArray();
        _mappers = mappers.ToArray();
        _deserializer = deserializer ?? throw new ArgumentNullException(nameof(deserializer));
        _fileReader = fileReader ?? throw new ArgumentNullException(nameof(fileReader));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _contextActionMappers =
            [
                new ContextActionExecuteScriptV1Processor(),
                new ContextActionSetVariableV1Processor(),
                new ContextActionEvaluateVariableV1Processor(),
                new ContextActionRenderVariableV1Processor(),
                new ContextActionLoadFileV1Processor(_fileReader),
            ];
    }

    public async IAsyncEnumerable<UserInterfaceRepositoryActionBase> CreateMenuAsync(IRepository repository, string filename)
    {
        ActionMenuGenerationContext context = await CreateActionMenuGenerationContext(repository).ConfigureAwait(false);

        ActionMenuRoot actions = await LoadAsync(filename).ConfigureAwait(false);

        // process context (vars + methods)
        await context.AddRepositoryContextAsync(actions.Context).ConfigureAwait(false);

        // process actions
        await foreach (UserInterfaceRepositoryActionBase item in context.AddActionMenusAsync(actions.ActionMenu).ConfigureAwait(false))
        {
            yield return item;
        }
    }

    public async Task<IEnumerable<string>> GetTagsAsync(IRepository repository, string filename)
    {
        ActionMenuGenerationContext context = await CreateActionMenuGenerationContext(repository).ConfigureAwait(false);

        // load yaml
        TagsRoot actions = await LoadTagsAsync(filename).ConfigureAwait(false);

        if (actions.Tags == null)
        {
            return [];
        }

        // process context (vars + methods)
        await context.AddRepositoryContextAsync(actions.Context).ConfigureAwait(false);

        // process tags
        return await context.GetTagsAsync(actions.Tags).ConfigureAwait(false);
    }

    private async Task<ActionMenuGenerationContext> CreateActionMenuGenerationContext(IRepository repository)
    {
        // force offloading to background thread (or use Task.Run(), or TaskFactory.....
        await Task.Yield();
        
        _logger.LogTrace("CreateActionMenuGenerationContext ActionMenuGenerationContext ctor");
        var actionMenuGenerationContext = new ActionMenuGenerationContext(_templateParser, _fileSystem, _plugins, _mappers, _deserializer, _contextActionMappers);
        actionMenuGenerationContext.Initialize(repository);
        return actionMenuGenerationContext;
    }

    private async Task<ActionMenuRoot> LoadAsync(string filename)
    {
        ActionMenuRoot? actions = await _fileReader.DeserializeRoot(filename).ConfigureAwait(false);
        return actions ?? throw new NotImplementedException("Could not deserialize file");
    }

    private async Task<TagsRoot> LoadTagsAsync(string filename)
    {
        TagsRoot? actions = await _fileReader.DeserializeTagsRoot(filename).ConfigureAwait(false);
        return actions ?? throw new NotImplementedException("Could not deserialize file");
    }
}