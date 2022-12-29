namespace RepoM.Plugin.Heidi.Internal.Config;
internal class RepomHeidiConfigV1 : RepomHeidiConfig
{
    public string[] Repositories { get; set; }

    public int Order { get; set; }

    public string Name { get; set; }

    public string Environment { get; set; }

    public string Application { get; set; }
}