namespace RepoM.Plugin.Heidi.ActionMenu.Context;

using System;

/// <summary>
/// Database configuration.
/// </summary>
internal readonly record struct DatabaseConfiguration
{
    public DatabaseConfiguration()
    {
    }

    public string Name { get; init; } = string.Empty;

    public string Host { get; init; } = string.Empty;

    public string User { get; init; } = string.Empty;

    public string Password { get; init; } = string.Empty;

    public int Port { get; init; } = 0;

    /// <summary>
    /// Use Windows authentication. (MSSQL, MySQL and MariaDB only).
    /// </summary>
    public bool UsesWindowsAuthentication { get; init; } = false;

    /// <summary>
    /// Comment within HeidiSQL.
    /// </summary>
    public string Comment { get; init; } = string.Empty;

    /// <summary>
    /// Databases, separated by semicolon. Single database on PostgreSQL. Interbase and Firebird expect a local file here.
    /// </summary>
    public string[] Databases { get; init; } = Array.Empty<string>();
}