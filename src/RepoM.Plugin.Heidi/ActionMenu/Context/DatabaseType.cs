namespace RepoM.Plugin.Heidi.ActionMenu.Context;

internal readonly record struct DatabaseType
{
    public DatabaseType()
    {
    }

    public string Name { get; init; } = string.Empty;

    public string Protocol { get; init; } = string.Empty;
}