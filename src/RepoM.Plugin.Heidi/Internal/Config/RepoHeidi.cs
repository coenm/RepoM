namespace RepoM.Plugin.Heidi.Internal.Config;

internal readonly record struct RepoHeidi
{
    public RepoHeidi(string key)
    {
        HeidiKey = key;
    }

    public string HeidiKey { get; }

    public string Repository { get; init; } = string.Empty;

    public int Order { get; init; } = int.MaxValue;

    public string Name { get; init; } = string.Empty;
    
    public string[] Tags { get; init; } = [];
}