namespace RepoM.Plugin.Heidi.Internal;

internal interface IHeidiSettings
{
    string ConfigPath { get; }

    string ConfigFilename { get; }

    string DefaultExe { get; }
}