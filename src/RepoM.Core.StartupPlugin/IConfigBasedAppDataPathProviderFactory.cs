namespace RepoM.Core.StartupPlugin;

using System.Threading.Tasks;
using RepoM.Core.Plugin.Common;

/// <summary>Creates the configuration paths before logging and application composition begin.</summary>
public interface IConfigBasedAppDataPathProviderFactory
{
    /// <summary>Creates the paths and completes any required profile preparation.</summary>
    /// <returns>The paths to use for this application instance.</returns>
    Task<IAppDataPathProvider> CreateAsync();
}
