namespace RepoM.Plugin.Heidi;

using JetBrains.Annotations;
using RepoM.Api.IO.ModuleBasedRepositoryActionProvider;
using RepoM.Core.Plugin;
using RepoM.Core.Plugin.VariableProviders;
using RepoM.Plugin.Heidi.Internal;
using SimpleInjector;
using SimpleInjector.Packaging;
using RepoM.Plugin.Heidi.VariableProviders;
using RepoM.Plugin.Heidi.ActionProvider;
using RepoM.Core.Plugin.RepositoryActions;
using RepoM.Plugin.Heidi.RepositoryActions;

[UsedImplicitly]
public class HeidiPackage : IPackage
{
    public void RegisterServices(Container container)
    {
        RegisterPluginHooks(container);
        RegisterInternals(container);
    }

    private static void RegisterPluginHooks(Container container)
    {
        // repository actions
        container.Collection.Append<IActionDeserializer, ActionHeidiDatabasesV1Deserializer>(Lifestyle.Singleton);
        container.Collection.Append<IActionToRepositoryActionMapper, ActionHeidiDatabasesV1Mapper>(Lifestyle.Singleton);

        // ordering
        // (see Statistics for example)

        // action executor
        // (see Statistics for example)
        container.Register(typeof(IActionExecutor<>), typeof(StartProcessActionExecutor), Lifestyle.Singleton);

        // variable provider
        container.Collection.Append<IVariableProvider, HeidiDbVariableProvider>(Lifestyle.Singleton);

        // module
        container.Collection.Append<IModule, HeidiModule>(Lifestyle.Singleton);
    }

    private static void RegisterInternals(Container container)
    {
        container.Register<HeidiConfigurationService>(Lifestyle.Singleton);
        container.Register<HeidiPortableConfigReader>(Lifestyle.Singleton);
    }
}