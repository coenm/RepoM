namespace RepoM.Plugin.Heidi.Internal;

using System;

[Obsolete("Will be removed in next major version.")]
internal class EnvironmentVariablesHeidiSettings : IHeidiSettings
{
    private const string ENV_VAR_PREFIX = "REPOM_HEIDI_";

    public string ConfigPath { get; } = DetermineValueUsingEnvironment("CONFIG_PATH", "C:\\StandAloneProgramFiles\\HeidiSQL_12.3_64_Portable\\");

    public string ConfigFilename { get; } = DetermineValueUsingEnvironment("CONFIG_FILENAME", "portable_settings.txt");

    public string DefaultExe { get; } = DetermineValueUsingEnvironment("EXE", "heidi.exe");

    private static string DetermineValueUsingEnvironment(string envVarName, string defaultValue)
    {
        try
        {
            var result = Environment.GetEnvironmentVariable(ENV_VAR_PREFIX + envVarName) ?? Environment.GetEnvironmentVariable(ENV_VAR_PREFIX + envVarName, EnvironmentVariableTarget.User);
            return string.IsNullOrWhiteSpace(result) ? defaultValue : result;
        }
        catch (Exception)
        {
            return defaultValue;
        }
    }
}