namespace RepoM.ActionMenu.Core.PublicApi;

using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using RepoM.ActionMenu.Core.Misc;
using RepoM.ActionMenu.Core.Services;
using RepoM.ActionMenu.Interface.Scriban;

public class Factory
{
    private readonly IFileSystem _fileSystem;
    private readonly ITemplateContextRegistration[] _plugins;
    private readonly ITemplateParser _templateParser = new FixedTemplateParser();

    public Factory(IFileSystem fileSystem, IEnumerable<ITemplateContextRegistration> plugins)
    {
        _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
        _plugins = plugins?.ToArray() ?? throw new ArgumentNullException(nameof(plugins));
    }

    public IUserInterfaceActionMenuFactory Create(string filename)
    {
        var result = new UserInterfaceActionMenuFactory(_fileSystem, _templateParser, _plugins, filename);
        return result;
    }
}