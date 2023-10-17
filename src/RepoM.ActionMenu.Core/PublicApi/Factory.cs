namespace RepoM.ActionMenu.Core.PublicApi;

using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using RepoM.ActionMenu.Core.Misc;
using RepoM.ActionMenu.Core.Services;
using RepoM.ActionMenu.Core.Yaml.Serialization;
using RepoM.ActionMenu.Interface.Scriban;
using RepoM.ActionMenu.Interface.YamlModel;
using RepoM.Core.Plugin.RepositoryOrdering.Configuration;

public class Factory
{
    private readonly IFileSystem _fileSystem;
    private readonly IKeyTypeRegistration<IMenuAction>[] _registrations;
    private readonly IActionToRepositoryActionMapper[] _mappers;
    private readonly ITemplateContextRegistration[] _plugins;
    private readonly ITemplateParser _templateParser = new FixedTemplateParser();
    private readonly ActionMenuDeserializer _deserializer;

    public Factory(
        IFileSystem fileSystem,
        IEnumerable<ITemplateContextRegistration> plugins,
        IEnumerable<IKeyTypeRegistration<IMenuAction>> registrations,
        IEnumerable<RepoM.ActionMenu.Interface.YamlModel.IActionToRepositoryActionMapper> mappers)
    {
        _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
        _mappers = mappers.ToArray() ?? throw new ArgumentNullException(nameof(mappers));
        _plugins = plugins.ToArray() ?? throw new ArgumentNullException(nameof(plugins));
        _deserializer = new ActionMenuDeserializer(registrations);
    }

    public IUserInterfaceActionMenuFactory Create()
    {
        return new UserInterfaceActionMenuFactory(_fileSystem, _templateParser, _plugins, _mappers, _deserializer);
    }
}