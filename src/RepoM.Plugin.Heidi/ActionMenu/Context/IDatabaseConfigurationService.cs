namespace RepoM.Plugin.Heidi.ActionMenu.Context;

using System.Collections.Generic;

internal interface IDatabaseConfigurationService
{
    IEnumerable<DatabaseConfiguration> GetDatabases();
}