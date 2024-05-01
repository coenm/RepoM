namespace RepoM.Plugin.Heidi.ActionMenu.Context;

internal readonly record struct Database
{
    public Database()
    {
    }

    public string Key { get; internal init; } = null!;

    public string Host { get; internal init; } = null!;

    public string User { get; internal init; } = null!;

    public string Password { get; internal init; } = null!;

    public int Port { get; internal init; } = 0;

    /// <summary>
    /// Use Windows authentication. (MSSQL, MySQL and MariaDB only).
    /// </summary>
    public bool UsesWindowsAuthentication { get; internal init; }

    public DatabaseType DatabaseType { get; internal init; }

    /// <summary>
    /// Library or provider (added in v11.1), Depends on the given network protocol
    /// https://www.heidisql.com/help.php#commandline
    /// </summary>
    public string Library { get; internal init; } = string.Empty;

    public string Comment { get; internal init; } = string.Empty;

    /// <summary>
    /// Databases. Single database on PostgreSQL. Interbase and Firebird expect a local file here.
    /// </summary>
    public string[] Databases { get; internal init; } = [];
}