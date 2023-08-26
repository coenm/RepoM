namespace RepoM.Api.IO.ModuleBasedRepositoryActionProvider.FileCache;

using System.Collections.Generic;
using System.Runtime.Caching;
using DotNetEnv;

internal class EnvFileStore : FileStore<Dictionary<string, string>>
{
    public EnvFileStore(ObjectCache cache) : base(cache)
    {
    }

    public IDictionary<string, string> TryGet(string filename)
    {
        Dictionary<string, string>? result = Get(filename);

        if (result != null)
        {
            return result;
        }

        IEnumerable<KeyValuePair<string, string>>? envResult = Env.Load(filename, new LoadOptions(setEnvVars: false));

        Dictionary<string, string>? fileContents = envResult == null ? new Dictionary<string, string>(0) : envResult.ToDictionary();

        return AddOrGetExisting(filename, fileContents);
    }
}