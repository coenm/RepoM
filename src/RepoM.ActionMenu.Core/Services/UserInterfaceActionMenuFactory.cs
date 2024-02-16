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
        _plugins = plugins.ToArray() ?? throw new ArgumentNullException(nameof(plugins));
        _mappers = mappers.ToArray() ?? throw new ArgumentNullException(nameof(mappers));
        _deserializer = deserializer ?? throw new ArgumentNullException(nameof(deserializer));
        _fileReader = fileReader ?? throw new ArgumentNullException(nameof(fileReader));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async IAsyncEnumerable<UserInterfaceRepositoryActionBase> CreateMenuAsync(IRepository repository, string filename)
    {
        _logger.LogTrace("CreateMenuAsync CreateActionMenuGenerationContext");
        ActionMenuGenerationContext context = await CreateActionMenuGenerationContext(repository).ConfigureAwait(false);

        // load yaml
        _logger.LogTrace("CreateActionMenuGenerationContext LoadAsync");
        Root actions = await LoadAsync(filename).ConfigureAwait(false);

        // process context (vars + methods)
        _logger.LogTrace("CreateActionMenuGenerationContext AddRepositoryContextAsync");
        await context.AddRepositoryContextAsync(actions.Context).ConfigureAwait(false);

        // process actions
        _logger.LogTrace("CreateActionMenuGenerationContext foreach AddActionMenusAsync");
        await foreach (UserInterfaceRepositoryActionBase item in context.AddActionMenusAsync(actions.ActionMenu).ConfigureAwait(false))
        {
            _logger.LogTrace("CreateActionMenuGenerationContext foreach inner");
            yield return item;
        }

        _logger.LogTrace("CreateMenuAsync Done");
    }

    public async Task<IEnumerable<string>> GetTagsAsync(IRepository repository, string filename)
    {
        var context = new ActionMenuGenerationContext(repository, _templateParser, _fileSystem, _plugins, _mappers, _deserializer, _fileReader);

        // load yaml
        Root actions = await LoadAsync(filename).ConfigureAwait(false);

        if (actions.Tags == null)
        {
            return Array.Empty<string>();
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
        return new ActionMenuGenerationContext(repository, _templateParser, _fileSystem, _plugins, _mappers, _deserializer, _fileReader);
    }

    private async Task<Root> LoadAsync(string filename)
    {
        Root? actions = await _fileReader.DeserializeRoot(filename).ConfigureAwait(false);
        return actions ?? throw new NotImplementedException("Could not deserialize file");
    }
}