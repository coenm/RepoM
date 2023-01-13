namespace RepoM.Plugin.Heidi.Internal.Config;

using System;

internal struct HeidiSingleDatabaseConfiguration
{
    public HeidiSingleDatabaseConfiguration(string key)
    {
        Key = key ?? throw new ArgumentNullException(nameof(key));
    }

    public string Key { get; }

    public string Host { get; set; } = string.Empty;

    public string User { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    public int Port { get; set; } = 0;

    /// <summary>
    /// Use Windows authentication: 1 or 0. (MSSQL, MySQL and MariaDB only).
    /// </summary>
    public bool WindowsAuth { get; set; } = false;

    /// <summary>
    /// Network protocol type:
    /// 0 = MariaDB/MySQL (TCP/IP)
    /// 1 = MariaDB/MySQL(named pipe)
    /// 2 = MariaDB/MySQL(SSH tunnel)
    /// 3 = MSSQL(named pipe)
    /// 4 = MSSQL(TCP/IP)
    /// 5 = MSSQL(SPX/IPX)
    /// 6 = MSSQL(Banyan VINES)
    /// 7 = MSSQL(Windows RPC)
    /// 8 = PostgreSQL(TCP/IP)
    /// 9 = PostgreSQL(SSH tunnel)
    /// 10 = SQLite
    /// 11 = ProxySQL Admin
    /// 12 = Interbase(TCP/IP)
    /// 13 = Interbase(local)
    /// 14 = Firebird(TCP/IP)
    /// 15 = Firebird(local)
    /// https://www.heidisql.com/help.php#commandline
    /// </summary>
    public int NetType { get; set; } = 0;

    /// <summary>
    /// Library or provider (added in v11.1), Depends on the given network protocol
    /// https://www.heidisql.com/help.php#commandline
    /// </summary>
    public string Library { get; set; } = string.Empty;

    public string Comment { get; set; } = string.Empty;

    /// <summary>
    /// Databases, separated by semicolon. Single database on PostgreSQL. Interbase and Firebird expect a local file here.
    /// </summary>
    public string[] Databases { get; set; } = Array.Empty<string>();
}