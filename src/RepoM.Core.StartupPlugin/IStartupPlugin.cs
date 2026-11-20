namespace RepoM.Core.StartupPlugin;

using System.Threading.Tasks;
using SimpleInjector;

/// <summary>Customizes the startup container before any startup dependencies are resolved.</summary>
/// <remarks>
/// Implementations must be public, concrete, and have a public parameterless constructor.
/// Register or decorate services only; resolving services locks the container prematurely.
/// This container is not the application's container.
/// </remarks>
public interface IStartupPlugin
{
    /// <summary>Registers replacements or decorators after the core defaults have been registered.</summary>
    /// <param name="container">The dedicated, still mutable startup container.</param>
    /// <returns>A task that completes when all registrations have been added.</returns>
    Task RegisterStartupDependenciesAsync(Container container);
}
