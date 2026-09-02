namespace RepoM.Plugin.Mcp.PersistentConfiguration;

using System;
using RepoM.Core.Plugin;

/// <remarks>DO NOT CHANGE PROPERTYNAMES, TYPES, or VISIBILITIES</remarks>
/// <summary>Module configuration (version 1)</summary>
[ModuleConfiguration(VERSION)]
public class McpConfigV1
{
    internal const int VERSION = 1;

    /// <summary>
    /// The port number on which the MCP server listens for HTTP connections. Defaults to 17823.
    /// </summary>
    public int? Port { get; init; }

    /// <summary>
    /// Optional API key for authenticating MCP clients. When set, clients must include this key in the `X-Api-Key` request header.
    /// A random key is generated on first startup when left empty.
    /// </summary>
    public string? ApiKey { get; init; }

    [ModuleConfigurationDefaultValueFactoryMethod]
    internal static McpConfigV1 CreateDefault()
    {
        return new McpConfigV1
        {
            Port = 17823,
            ApiKey = GenerateApiKey(),
        };
    }

    internal static string GenerateApiKey()
    {
        Span<byte> bytes = stackalloc byte[32];
        System.Security.Cryptography.RandomNumberGenerator.Fill(bytes);
        return Convert.ToBase64String(bytes);
    }
}
