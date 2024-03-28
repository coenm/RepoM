namespace RepoM.ActionMenu.Core.Tests.Plugin;

using System;

public readonly record struct DummyConfig()
{
    public string Key { get; init; } = string.Empty;

    public string Host { get; init; } = string.Empty;

    public int Port { get; init; } = 0;

    public bool IsOnline { get; init; } = false;

    public string[] DatabaseNames { get; init; } = Array.Empty<string>();
}