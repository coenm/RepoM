namespace RepoM.Plugin.EverythingFileSearch;

using JetBrains.Annotations;
using RepoM.Api.IO;
using SimpleInjector;
using SimpleInjector.Packaging;

[UsedImplicitly]
public class EverythingModule : IPackage
{
    public void RegisterServices(Container container)
    {
        container.Collection.Append<ISingleGitRepositoryFinderFactory, EverythingGitRepositoryFinderFactory>(Lifestyle.Singleton);
    }
}