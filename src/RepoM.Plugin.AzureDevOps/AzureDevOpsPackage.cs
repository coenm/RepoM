namespace RepoM.Plugin.AzureDevOps;

using JetBrains.Annotations;
using RepoM.Api.IO.ModuleBasedRepositoryActionProvider;
using RepoM.Core.Plugin;
using RepoM.Core.Plugin.RepositoryFiltering;
using RepoM.Plugin.AzureDevOps.ActionProvider;
using RepoM.Plugin.AzureDevOps.Internal;
using RepoM.Plugin.AzureDevOps.RepositoryFiltering;
using SimpleInjector;
using SimpleInjector.Packaging;

[UsedImplicitly]
public class AzureDevOpsPackage : IPackage
{
    public void RegisterServices(Container container)
    {
        container.Collection.Append<IActionDeserializer, ActionAzureDevOpsCreatePullRequestsV1Deserializer>(Lifestyle.Singleton);
        container.Collection.Append<IActionDeserializer, ActionAzureDevOpsGetPullRequestsV1Deserializer>(Lifestyle.Singleton);
        container.Collection.Append<IActionToRepositoryActionMapper, ActionAzureDevOpsCreatePullRequestsV1Mapper>(Lifestyle.Singleton);
        container.Collection.Append<IActionToRepositoryActionMapper, ActionAzureDevOpsGetPullRequestsV1Mapper>(Lifestyle.Singleton);

        container.Register<IAzureDevOpsPullRequestService, AzureDevOpsPullRequestService>(Lifestyle.Singleton);

        container.Collection.Append<IModule, AzureDevOpsModule>(Lifestyle.Singleton);
        container.Collection.Append<IQueryMatcher>(() => new HasPullRequestsMatcher(container.GetInstance<IAzureDevOpsPullRequestService>(), true), Lifestyle.Singleton);
    }
}