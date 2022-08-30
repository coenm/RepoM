namespace RepoM.Plugin.SonarCloud;

using ExpressionStringEvaluator.Methods;
using JetBrains.Annotations;
using RepoM.Api;
using RepoM.Api.IO.ModuleBasedRepositoryActionProvider;
using RepoM.Core.Plugin;
using SimpleInjector;
using SimpleInjector.Packaging;

[UsedImplicitly]
public class SonarCloudPackage : IPackage
{
    public void RegisterServices(Container container)
    {
        container.Collection.Append<IActionDeserializer, ActionSonarCloudSetFavoriteV1Deserializer>(Lifestyle.Singleton);
        container.Collection.Append<IActionToRepositoryActionMapper, ActionSonarCloudV1Mapper>(Lifestyle.Singleton);
        container.Collection.Append<IMethod, SonarCloudIsFavoriteMethod>(Lifestyle.Singleton);

        container.Register<SonarCloudFavoriteService>(Lifestyle.Singleton);
        container.Collection.Append<IModule, SonarCloudModule>(Lifestyle.Singleton);
    }
}