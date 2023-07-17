namespace RepoM.Plugin.SonarCloud;

internal class SonarCloudConfiguration : ISonarCloudConfiguration
{
    public SonarCloudConfiguration(string? url, string? pat)
    {
        PersonalAccessToken = pat;
        BaseUrl = url ?? "https://sonarcloud.io";
    }

    public string? PersonalAccessToken { get; }

    public string? BaseUrl { get; }
}