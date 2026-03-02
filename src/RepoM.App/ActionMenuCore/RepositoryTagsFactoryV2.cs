namespace RepoM.App.ActionMenuCore;

using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Threading.Tasks;
using RepoM.ActionMenu.Core;
using RepoM.Api.IO.ModuleBasedRepositoryActionProvider;
using RepoM.Core.Plugin.Common;
using RepoM.Core.Plugin.Repository;

// todo move to beter location
public class RepositoryTagsFactoryV2 : IRepositoryTagsFactory
{
    private readonly IUserInterfaceActionMenuFactory _newStyleActionMenuFactory;
    private readonly string _filename;

    public RepositoryTagsFactoryV2(
        IFileSystem fileSystem,
        IUserInterfaceActionMenuFactory newStyleActionMenuFactory,
        IAppDataPathProvider appDataPathProvider)
    {
        _ = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
        _newStyleActionMenuFactory = newStyleActionMenuFactory ?? throw new ArgumentNullException(nameof(newStyleActionMenuFactory));
        _ = appDataPathProvider ?? throw new ArgumentNullException(nameof(appDataPathProvider));

        _filename = fileSystem.Path.Combine(appDataPathProvider.AppDataPath, "TagsV2.yaml");
    }

    public Task<IEnumerable<string>> GetTagsAsync(IRepository repository)
    {
        return _newStyleActionMenuFactory.GetTagsAsync(repository, _filename);
    }
}
