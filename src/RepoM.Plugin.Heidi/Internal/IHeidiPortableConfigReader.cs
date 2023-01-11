namespace RepoM.Plugin.Heidi.Internal;

using System.Collections.Generic;
using System.Threading.Tasks;
using RepoM.Plugin.Heidi.Internal.Config;

internal interface IHeidiPortableConfigReader
{
    Task<List<HeidiSingleDatabaseConfiguration>> ParseAsync(string filename);

    // Task<Dictionary<string, RepomHeidiConfig>> ReadConfigsAsync(string filename);
}