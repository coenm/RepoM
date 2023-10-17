namespace RepoM.ActionMenu.Core.Services;

using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using System.Threading.Tasks;
using RepoM.ActionMenu.Core.Misc;
using RepoM.ActionMenu.Core.Model;
using RepoM.ActionMenu.Core.PublicApi;
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

    public UserInterfaceActionMenuFactory(
        IFileSystem fileSystem, 
        ITemplateParser templateParser,
        ITemplateContextRegistration[] plugins,
        IEnumerable<IActionToRepositoryActionMapper> mappers,
        IActionMenuDeserializer deserializer)
    {
        _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
        _templateParser = templateParser ?? throw new ArgumentNullException(nameof(templateParser));
        _plugins = plugins ?? throw new ArgumentNullException(nameof(plugins));
        _mappers = mappers.ToArray() ?? throw new ArgumentNullException(nameof(mappers));
        _deserializer = deserializer ?? throw new ArgumentNullException(nameof(deserializer));
    }

    public async Task<IEnumerable<UserInterfaceRepositoryActionBase>> CreateMenuAsync(IRepository repository, string filename)
    {
        var context = new ActionMenuGenerationContext(repository, _templateParser, _fileSystem, _plugins, _mappers, _deserializer);

        // load yaml
        Root actions = await LoadAsync(filename).ConfigureAwait(false);

        // process context (vars + methods)
        await context.AddRepositoryContextAsync(actions.Context).ConfigureAwait(false);

        // process actions
        return await context.AddActionMenusAsync(actions.ActionMenu).ConfigureAwait(false);
    }
    
    public async Task<IEnumerable<string>> GetTagsAsync(IRepository repository, string filename)
    {
        var context = new ActionMenuGenerationContext(repository, _templateParser, _fileSystem, _plugins, _mappers, _deserializer);

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

    private async Task<Root> LoadAsync(string filename)
    {
        var yaml = await _fileSystem.File.ReadAllTextAsync(filename).ConfigureAwait(false);
        Root? actions = _deserializer.DeserializeRoot(yaml);
        return actions ?? throw new NotImplementedException("Could not deserialize file");
    }
}