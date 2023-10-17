namespace RepoM.Plugin.SonarCloud.RepositoryCommands;

using System;
using JetBrains.Annotations;
using RepoM.Core.Plugin.Repository;
using RepoM.Core.Plugin.RepositoryActions;

[UsedImplicitly] // todo not sure if this should be public or not.
public class CopyToClipboardCommandExecutor : ICommandExecutor<SonarCloudSetFavoriteRepositoryCommand>
{
    private readonly ISonarCloudFavoriteService _service;

    // todo not sure if this should be public or not.
    internal CopyToClipboardCommandExecutor(ISonarCloudFavoriteService service)
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