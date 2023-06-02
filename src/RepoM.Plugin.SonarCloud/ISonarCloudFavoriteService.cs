namespace RepoM.Plugin.SonarCloud;

using System.Threading.Tasks;

internal interface ISonarCloudFavoriteService
{
    bool IsInitialized { get; }
    Task InitializeAsync();
    Task SetFavorite(string repoKey);
    bool IsFavorite(string repoKey);
}