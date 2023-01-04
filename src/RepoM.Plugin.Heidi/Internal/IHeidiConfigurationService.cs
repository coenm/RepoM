namespace RepoM.Plugin.Heidi.Internal;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RepoM.Core.Plugin.Repository;
using RepoM.Plugin.Heidi.Interface;

internal interface IHeidiConfigurationService
{
    event EventHandler ConfigurationUpdated;

    Task InitializeAsync();

    IEnumerable<HeidiConfiguration> GetByRepository(IRepository repository);

    IEnumerable<HeidiConfiguration> GetByKey(string key);
}