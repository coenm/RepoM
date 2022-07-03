namespace RepoM.Plugin.SonarCloud;

using RepoM.Api.Common.IO.ModuleBasedRepositoryActionProvider.Data;

public class RepositoryActionSonarCloudSetFavoriteV1 : RepositoryAction
{
    public string? Project { get; set; }
}