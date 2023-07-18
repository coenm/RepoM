namespace RepoM.Plugin.SonarCloud;

internal interface ISonarCloudConfiguration
{
    string? PersonalAccessToken { get; }

    string? BaseUrl { get; }

}