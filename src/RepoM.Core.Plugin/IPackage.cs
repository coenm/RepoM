namespace RepoM.Core.Plugin;

using System.Threading.Tasks;
using SimpleInjector;

/// <summary>
/// Contract for a RepoM Plugin
/// </summary>
/// <example>
/// The following example shows an implementation of an <see cref="IPackage"/>.
/// <code lang="cs"><![CDATA[
/// public class MyPluginPackage : IPackage
/// {
///     public Task RegisterServicesAsync(Container container, IPackageConfiguration packageConfiguration)
///     {
///         container.Register<IUserRepository, DatabaseUserRepository>();
///         container.Register<ICustomerRepository, DatabaseCustomerRepository>();
///         return Task.CompletedTask;
///     }
/// }
/// ]]></code>
/// </example>
public interface IPackage
{
    public string Name { get; }

    /// <summary>Registers the set of services in the specified <paramref name="container"/>.</summary>
    /// <param name="container">The container the set of services is registered into.</param>
    /// <param name="packageConfiguration">The package configuration.</param>
    Task RegisterServicesAsync(Container container, IPackageConfiguration packageConfiguration);
}