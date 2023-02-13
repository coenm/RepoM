namespace RepoM.Plugin.AzureDevOps;

using JetBrains.Annotations;
using RepoM.Api.IO.ModuleBasedRepositoryActionProvider;
using RepoM.Core.Plugin;
using RepoM.Core.Plugin.RepositoryFiltering;
using SimpleInjector;
using SimpleInjector.Packaging;

[UsedImplicitly]
public class AzureDevOpsPackage : IPackage
{
    public void RegisterServices(Container container)
    {
        container.Collection.Append<IActionDeserializer, ActionAzureDevOpsPullRequestsV1Deserializer>(Lifestyle.Singleton);
        container.Collection.Append<IActionToRepositoryActionMapper, ActionAzureDevOpsPullRequestsV1Mapper>(Lifestyle.Singleton);

        container.Register<AzureDevOpsPullRequestService>(Lifestyle.Singleton);

        container.Collection.Append<IModule, AzureDevOpsModule>(Lifestyle.Singleton);
        container.Collection.Append<IQueryMatcher>(() => new HasPullRequestsMatcher(container.GetInstance<AzureDevOpsPullRequestService>(), true), Lifestyle.Singleton);
    }
}