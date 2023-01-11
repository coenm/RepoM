namespace RepoM.Plugin.Heidi.Internal.Config;

using System;

internal struct RepoHeidi
{
    public RepoHeidi(string key)
    {
        HeidiKey = key;
    }

    public string HeidiKey { get; }

    public string Repository { get; set; } = string.Empty;

    public int Order { get; set; } = int.MaxValue;

    public string Name { get; set; } = string.Empty;
    
    public string[] Tags { get; set; } = Array.Empty<string>();
}