namespace RepoM.Plugin.Heidi.PersistentConfiguration;

using RepoM.Core.Plugin;

/// <remarks>DO NOT CHANGE PROPERTYNAMES, TYPES, or VISIBILITIES</remarks>
/// <summary>Module configuration (version 1)</summary>
[ModuleConfiguration]
public class HeidiConfigV1
{
    /// <summary>
    /// The full directory where the portable configuration file is stored.
    /// </summary>
    public string? ConfigPath { get; init; }

    /// <summary>
    /// The portable-configurration filename (without path). Most likely `portable_settings.txt`
    /// </summary>
    public string? ConfigFilename { get; init; }

    /// <summary>
    /// The full executable of Heidi.
    /// </summary>
    public string? ExecutableFilename { get; init;}

    [ModuleConfigurationDefaultValueFactoryMethod]
    internal static HeidiConfigV1 CreateDefault()
    {
        return new HeidiConfigV1();
    }
}