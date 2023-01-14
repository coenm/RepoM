namespace RepoM.Plugin.EverythingFileSearch;

using JetBrains.Annotations;
using RepoM.Core.Plugin.RepositoryFinder;
using SimpleInjector;
using SimpleInjector.Packaging;

[UsedImplicitly]
public class EverythingPackage : IPackage
{
    public void RegisterServices(Container container)
    {
        container.Collection.Append<ISingleGitRepositoryFinderFactory, EverythingGitRepositoryFinderFactory>(Lifestyle.Singleton);
    }
}