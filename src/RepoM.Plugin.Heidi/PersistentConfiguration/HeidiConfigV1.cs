namespace RepoM.Plugin.Heidi.PersistentConfiguration;

/// <remarks>DO NOT CHANGE PROPERTYNAMES, TYPES, or VISIBILITIES</remarks>
/// <summary>Module configuration (version 1)</summary>
public class HeidiConfigV1
{
    /// <summary>
    /// The full directory where the configuration is stored.
    /// </summary>
    public string? ConfigPath { get; init; }

    /// <summary>
    /// The configurration filename (without path)
    /// </summary>
    public string? ConfigFilename { get; init; }

    /// <summary>
    /// The full executable of Heidi.
    /// </summary>
    public string? ExecutableFilename { get; init;}
}