namespace RepoM.Plugin.SonarCloud.RepositoryCommands;

using System;
using JetBrains.Annotations;
using RepoM.Core.Plugin.Repository;
using RepoM.Core.Plugin.RepositoryActions;

[UsedImplicitly]
internal class SonarCloudSetFavoriteRepositoryCommandExecutor : ICommandExecutor<SonarCloudSetFavoriteRepositoryCommand>
{
    private readonly ISonarCloudFavoriteService _service;

    public SonarCloudSetFavoriteRepositoryCommandExecutor(ISonarCloudFavoriteService service)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
    }

    public void Execute(IRepository repository, SonarCloudSetFavoriteRepositoryCommand repositoryCommand)
    {
        try
        {
            _ = _service.SetFavorite(repositoryCommand.Key);
        }
        catch (Exception)
        {
            // ignore
        }
    }
}