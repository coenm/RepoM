namespace RepoM.Plugin.Heidi.Internal.Config;

internal class RepomHeidiConfig
{
    public string HeidiKey { get; set; }

    public string[] Repositories { get; set; }

    public int Order { get; set; }

    public string Name { get; set; }

    public string Environment { get; set; }
}