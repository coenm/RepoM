namespace RepoM.App.Services;

using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Threading.Tasks;
using RepoM.ActionMenu.Core;
using RepoM.ActionMenu.Interface.UserInterface;
using RepoM.Core.Plugin.Common;
using RepoM.Core.Plugin.Repository;
using RepoM.Core.Plugin.RepositoryActions.Commands;

internal class UserMenuActionMenuFactory : IUserMenuActionMenuFactory
{
    const string FILENAME = "RepositoryActionsV2.yaml";

    private readonly IFileSystem _fileSystem;
    private readonly IAppDataPathProvider _appDataPathProvider;
    private readonly IUserInterfaceActionMenuFactory _factory;

    public UserMenuActionMenuFactory(
        IFileSystem fileSystem,
        IAppDataPathProvider appDataPathProvider,
        IUserInterfaceActionMenuFactory userInterfaceActionMenuFactory)
    {
        _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
        _appDataPathProvider = appDataPathProvider ?? throw new ArgumentNullException(nameof(appDataPathProvider));
        _factory = userInterfaceActionMenuFactory ?? throw new ArgumentNullException(nameof(userInterfaceActionMenuFactory));
    }

    public async IAsyncEnumerable<UserInterfaceRepositoryActionBase> CreateMenuAsync(IRepository repository)
    {
        var fullFilename = System.IO.Path.Combine(_appDataPathProvider.AppDataPath, FILENAME);
        var fileExists = _fileSystem.File.Exists(fullFilename);

        if (!fileExists)
        {
            yield return new UserInterfaceRepositoryAction(FILENAME + " not found", repository)
                {
                    CanExecute = false,
                    ExecutionCausesSynchronizing = false,
                };
            yield return new UserInterfaceRepositoryAction("Restart RepoM to create " + FILENAME, repository)
                {
                    CanExecute = false,
                    ExecutionCausesSynchronizing = false,
                };
            yield return new UserInterfaceRepositoryAction("Open directory in explorer", repository)
                {
                    CanExecute = true,
                    ExecutionCausesSynchronizing = true,
                    RepositoryCommand = new OpenDirectoryCommand(_appDataPathProvider.AppDataPath),
                };
            yield break;
        }

        await foreach (UserInterfaceRepositoryActionBase item in _factory.CreateMenuAsync(repository, fullFilename).ConfigureAwait(false))
        {
            yield return item;
        }
    }
}