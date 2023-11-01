namespace RepoM.Plugin.Heidi.ActionMenu.Context;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using RepoM.ActionMenu.Interface.ActionMenuFactory;
using RepoM.ActionMenu.Interface.Attributes;
using RepoM.ActionMenu.Interface.Scriban;
using RepoM.Core.Plugin.Repository;
using RepoM.Plugin.Heidi.Internal;

/// <summary>
/// Provides variables provided by the Heidi module. The variables are accessable through `heidi`.
/// </summary>
[UsedImplicitly]
[ActionMenuModule("heidi")]
internal partial class HeidiDbVariables : TemplateContextRegistrationBase
{
    private const string NAME_MARIA_DB = "MariaDB/MySQL";
    private const string NAME_MS_SQL = "MSSQL";
    private const string NAME_POSTGRE_SQL = "PostgreSQL";
    private const string NAME_SQLITE = "SQLite";
    private const string NAME_PROXY_SQL = "ProxySQL Admin";
    private const string NAME_INTERBASE = "Interbase";
    private const string NAME_FIREBIRD = "Firebird";

    private const string PROTOCOL_TCP_IP = "TCP/IP";
    private const string PROTOCOL_NAMED_PIPE = "named pipe";
    private const string PROTOCOL_SSH = "SSH";
    private const string PROTOCOL_SPX_IPX = "SPX/IPX";
    private const string PROTOCOL_BANYAN = "Banyan VINES";
    private const string PROTOCOL_RPC = "Windows RPC";
    private const string PROTOCOL_LOCAL = "Local";

    private static readonly DatabaseType[] _types = new DatabaseType[]
        {
            /*  0 */ new() { Name = NAME_MARIA_DB, Protocol = PROTOCOL_TCP_IP, },
            /*  1 */ new() { Name = NAME_MARIA_DB, Protocol = PROTOCOL_NAMED_PIPE, },
            /*  2 */ new() { Name = NAME_MARIA_DB, Protocol = PROTOCOL_SSH, },
            /*  3 */ new() { Name = NAME_MS_SQL, Protocol = PROTOCOL_NAMED_PIPE, },
            /*  4 */ new() { Name = NAME_MS_SQL, Protocol = PROTOCOL_TCP_IP, },
            /*  5 */ new() { Name = NAME_MS_SQL, Protocol = PROTOCOL_SPX_IPX, },
            /*  6 */ new() { Name = NAME_MS_SQL, Protocol = PROTOCOL_BANYAN, },
            /*  7 */ new() { Name = NAME_MS_SQL, Protocol = PROTOCOL_RPC, },
            /*  8 */ new() { Name = NAME_POSTGRE_SQL, Protocol = PROTOCOL_TCP_IP, },
            /*  9 */ new() { Name = NAME_POSTGRE_SQL, Protocol = PROTOCOL_SSH, },
            /* 10 */ new() { Name = NAME_SQLITE, Protocol = string.Empty, },
            /* 11 */ new() { Name = NAME_PROXY_SQL, Protocol = string.Empty, },
            /* 12 */ new() { Name = NAME_INTERBASE, Protocol = PROTOCOL_TCP_IP, },
            /* 13 */ new() { Name = NAME_INTERBASE, Protocol = PROTOCOL_LOCAL, },
            /* 14 */ new() { Name = NAME_FIREBIRD, Protocol = PROTOCOL_TCP_IP, },
            /* 15 */ new() { Name = NAME_FIREBIRD, Protocol = PROTOCOL_LOCAL, },
        };

    private readonly IHeidiConfigurationService _service;

    public HeidiDbVariables(IHeidiConfigurationService service)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
    }

    /// <summary>
    /// Gets all known databases configured in the Heidi configuration related to the selected repository.
    /// </summary>
    /// <returns>An enumerable of databases.</returns>
    /// <example>
    /// <code>
    /// heidi.databases
    /// # heidi.databases()
    /// </code>
    /// <code>
    /// [ TODO ]
    /// </code>
    /// </example>
    [ActionMenuMember("databases")]
    public IEnumerable GetDatabases(IActionMenuGenerationContext context)
    {
        return GetByRepository(context.Repository);
    }
    
    private IEnumerable<DatabaseConfiguration> GetByRepository(IRepository repository)
    {
        return _service.GetByRepository(repository)
          .Select(item => new DatabaseConfiguration()
            {
                Database = new Database
                    {
                        Key = item.DbConfig.Key,
                        Databases = item.DbConfig.Databases.ToArray(),
                        Comment = item.DbConfig.Comment,
                        Host = item.DbConfig.Host,
                        Library = item.DbConfig.Library,
                        User = item.DbConfig.User,
                        Password = item.DbConfig.Password,
                        UsesWindowsAuthentication = item.DbConfig.WindowsAuth,
                        Port = item.DbConfig.Port,
                        DatabaseType = GetDatabaseType(item.DbConfig.NetType),
                    },
                Metadata = new MetaData
                    {
                        Name = item.Name,
                        Order = item.Order,
                        Tags = item.Tags.ToArray(),
                    },
            });
    }

    private static DatabaseType GetDatabaseType(int type)
    {
        if (type >= 0 && type < _types.Length)
        {
            return _types[type];
        }

        return new DatabaseType()
            {
                Name = type.ToString(),
                Protocol = string.Empty,
            };
    }
}