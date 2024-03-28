namespace RepoM.Plugin.Heidi.ActionMenu.Context;

/// <summary>
/// Database configuration.
/// </summary>
internal readonly record struct DatabaseConfiguration
{
    public DatabaseConfiguration()
    {
    }

    public MetaData Metadata { get; init; }

    public Database Database { get; init; }
}