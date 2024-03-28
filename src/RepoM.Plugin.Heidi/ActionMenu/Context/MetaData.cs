namespace RepoM.Plugin.Heidi.ActionMenu.Context;

using System;

internal readonly record struct MetaData
{
    public MetaData()
    {
    }

    public string Name { get; init; } = string.Empty;

    public int Order { get; init; } = 0;

    public string[] Tags { get; init; } = Array.Empty<string>();
}