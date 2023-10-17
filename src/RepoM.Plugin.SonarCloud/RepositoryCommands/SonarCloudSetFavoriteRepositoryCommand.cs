namespace RepoM.Plugin.SonarCloud.RepositoryCommands;

using RepoM.Core.Plugin.RepositoryActions;

public sealed class SonarCloudSetFavoriteRepositoryCommand : IRepositoryCommand
{
    internal SonarCloudSetFavoriteRepositoryCommand(string key)
    {
        Key = key;
    }

    public string Key { get; }
}