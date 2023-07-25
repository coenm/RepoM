namespace RepoM.Plugin.EverythingFileSearch;

using System.Threading.Tasks;
using JetBrains.Annotations;
using RepoM.Core.Plugin;
using RepoM.Core.Plugin.RepositoryFinder;
using SimpleInjector;

[UsedImplicitly]
public class EverythingPackage : IPackageWithConfiguration
{
    public string Name => "EverythingPackage";

    public Task RegisterServicesAsync(Container container, IPackageConfiguration packageConfiguration)
    {
        container.Collection.Append<ISingleGitRepositoryFinderFactory, EverythingGitRepositoryFinderFactory>(Lifestyle.Singleton);
        return Task.CompletedTask;
    }
}