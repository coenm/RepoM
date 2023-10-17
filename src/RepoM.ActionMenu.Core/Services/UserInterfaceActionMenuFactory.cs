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
using RepoM.ActionMenu.Core.Yaml.Serialization;
using RepoM.ActionMenu.Interface.Scriban;
using RepoM.ActionMenu.Interface.UserInterface;
using RepoM.ActionMenu.Interface.YamlModel;
using RepoM.Core.Plugin.Repository;
using RepoM.Core.Plugin.RepositoryOrdering.Configuration;

internal class UserInterfaceActionMenuFactory : IUserInterfaceActionMenuFactory
{
    private readonly IFileSystem _fileSystem;
    private readonly ITemplateParser _templateParser;
    private readonly ITemplateContextRegistration[] _plugins;
    private readonly IKeyTypeRegistration<IMenuAction>[] _registrations;
    private readonly IActionToRepositoryActionMapper[] _mappers;
    private readonly string _filename;
    private readonly IActionMenuDeserializer _deserializer;

    public UserInterfaceActionMenuFactory(
        IFileSystem fileSystem, 
        ITemplateParser templateParser,
        ITemplateContextRegistration[] plugins,
        IEnumerable<IKeyTypeRegistration<IMenuAction>> registrations,
        IEnumerable<RepoM.ActionMenu.Interface.YamlModel.IActionToRepositoryActionMapper> mappers,
        string filename)
    {
        _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
        _templateParser = templateParser ?? throw new ArgumentNullException(nameof(templateParser));
        _plugins = plugins ?? throw new ArgumentNullException(nameof(plugins));
        _registrations = registrations.ToArray() ?? throw new ArgumentNullException(nameof(registrations));
        _mappers = mappers.ToArray() ?? throw new ArgumentNullException(nameof(mappers));
        _filename = filename ?? throw new ArgumentNullException(nameof(filename));
        _deserializer = new ActionMenuDeserializer(_registrations);
    }

    public async Task<IEnumerable<UserInterfaceRepositoryActionBase>> CreateMenuAsync(IRepository repository)
    {
        var context = new ActionMenuGenerationContext(repository, _templateParser, _fileSystem, _plugins, _mappers, _deserializer);

        // load yaml
        Root actions = await context.LoadAsync(_filename).ConfigureAwait(false);

        // process context (vars + methods)
        await context.AddRepositoryContextAsync(actions.Context).ConfigureAwait(false);

        // process tags

        // process actions
        return await context.AddActionMenusAsync(actions.ActionMenu).ConfigureAwait(false);
    }
    
    public async Task<IEnumerable<string>> GetTagsAsync(IRepository repository)
    {
        var context = new ActionMenuGenerationContext(repository, _templateParser, _fileSystem, _plugins, _mappers, _deserializer);

        // load yaml
        Root actions = await context.LoadAsync(_filename).ConfigureAwait(false);

        if (actions.Tags == null)
        {
            return Array.Empty<string>();
        }

        // process context (vars + methods)
        await context.AddRepositoryContextAsync(actions.Context).ConfigureAwait(false);

        // process tags
        return await context.GetTagsAsync(actions.Tags).ConfigureAwait(false);
    }
}