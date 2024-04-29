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
        List<UserInterfaceRepositoryActionBase> result = await Task.Run(async () =>
            {
                List<UserInterfaceRepositoryActionBase> result = new();

                var fullFilename = System.IO.Path.Combine(_appDataPathProvider.AppDataPath, FILENAME);
                var fileExists = _fileSystem.File.Exists(fullFilename);


                if (!fileExists)
                {
                    result.Add(new UserInterfaceRepositoryAction(FILENAME + " not found", repository)
                        {
                            CanExecute = false,
                            ExecutionCausesSynchronizing = false,
                        });
                    result.Add(new UserInterfaceRepositoryAction("Restart RepoM to create " + FILENAME, repository)
                        {
                            CanExecute = false,
                            ExecutionCausesSynchronizing = false,
                        });
                    result.Add(new UserInterfaceRepositoryAction("Open directory in explorer", repository)
                        {
                            CanExecute = true,
                            ExecutionCausesSynchronizing = true,
                            RepositoryCommand = new OpenDirectoryCommand(_appDataPathProvider.AppDataPath),
                        });
                    return result;
                }

                await foreach (UserInterfaceRepositoryActionBase item in _factory.CreateMenuAsync(repository, fullFilename).ConfigureAwait(false))
                {
                    result.Add(item);
                }

                return result;
            });

        foreach (UserInterfaceRepositoryActionBase item in result)
        {
            yield return item;
        }
    }
}