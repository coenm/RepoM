namespace RepoM.Core.Plugin;

using System.Threading.Tasks;

/// <summary>
/// Contract for a RepoM Module
/// </summary>
public interface IModule
{
    /// <summary>
    /// Start the module.
    /// </summary>
    /// <returns>Task</returns>
    Task StartAsync();

    /// <summary>
    /// Stops the module.
    /// </summary>
    /// <returns>Task.</returns>
    Task StopAsync();
}