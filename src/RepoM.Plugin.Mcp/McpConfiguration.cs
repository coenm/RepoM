namespace RepoM.Plugin.Mcp;

internal interface IMcpConfiguration
{
    int Port { get; }

    string? ApiKey { get; }
}

internal class McpConfiguration : IMcpConfiguration
{
    public int Port { get; init; }

    public string? ApiKey { get; init; }
}
