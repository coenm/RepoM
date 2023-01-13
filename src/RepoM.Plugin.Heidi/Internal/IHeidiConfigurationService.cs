namespace RepoM.Plugin.Heidi.Internal;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;
using RepoM.Core.Plugin.Repository;
using RepoM.Plugin.Heidi.Interface;
using RepoM.Plugin.Heidi.Internal.Config;

internal interface IHeidiConfigurationService
{
    event EventHandler ConfigurationUpdated;

    Task InitializeAsync();

    ImmutableArray<HeidiSingleDatabaseConfiguration> GetAllDatabases();

    IEnumerable<RepositoryHeidiConfiguration> GetByRepository(IRepository repository);

    IEnumerable<RepositoryHeidiConfiguration> GetByKey(string key);
}