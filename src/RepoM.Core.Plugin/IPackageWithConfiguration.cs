namespace RepoM.Core.Plugin;

using System.Threading.Tasks;
using SimpleInjector;
using SimpleInjector.Packaging;

public interface IPackageWithConfiguration : IPackage
{
    public string Name { get; }

    /// <summary>Registers the set of services in the specified <paramref name="container"/>.</summary>
    /// <param name="container">The container the set of services is registered into.</param>
    Task RegisterServicesAsync(Container container, IPackageConfiguration packageConfiguration);
}