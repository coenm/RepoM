namespace RepoM.Plugin.Heidi.Internal;

internal class HeidiModuleConfiguration : IHeidiSettings
{
    public HeidiModuleConfiguration(string? configPath, string? configFilename, string? defaultExe)
    {
        ConfigPath = configPath ?? string.Empty;
        ConfigFilename = configFilename ?? string.Empty;
        DefaultExe = defaultExe ?? string.Empty;
    }

    public string ConfigPath { get; }

    public string ConfigFilename { get; }

    public string DefaultExe { get; }
}