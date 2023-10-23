namespace RepoM.Plugin.Heidi.ActionMenu.Context;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using RepoM.Plugin.Heidi.Internal;
using RepoM.Plugin.Heidi.Internal.Config;

internal class DatabaseConfigurationService : IDatabaseConfigurationService
{
    private readonly IHeidiConfigurationService _internalService;

    public DatabaseConfigurationService(IHeidiConfigurationService internalService)
    {
        _internalService = internalService ?? throw new ArgumentNullException(nameof(internalService));
    }

    public IEnumerable<DatabaseConfiguration> GetDatabases()
    {
        ImmutableArray<HeidiSingleDatabaseConfiguration> result = _internalService.GetAllDatabases();
        return result.Select(x => new DatabaseConfiguration
            {
                Name = x.Key,
                Host = x.Host,
                User = x.User,
                Password = x.Password,
                Port = x.Port,
                UsesWindowsAuthentication = x.WindowsAuth,
                Comment = x.Comment,
                Databases = x.Databases.ToArray(),
            });
    }
}