namespace RepoM.Plugin.Heidi.Internal.Config;

internal struct HeidiSingleLineConfiguration
{
    public string Key { get; set; }

    public string Type { get; set; }

    public string ContentType { get; set; }

    public string? Content { get; set; }
}