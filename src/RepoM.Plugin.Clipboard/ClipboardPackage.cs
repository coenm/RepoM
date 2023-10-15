namespace RepoM.Plugin.Clipboard;

using System.Threading.Tasks;
using JetBrains.Annotations;
using RepoM.Api.IO.ModuleBasedRepositoryActionProvider;
using RepoM.Api.IO.ModuleBasedRepositoryActionProvider.Data;
using RepoM.Core.Plugin;
using RepoM.Core.Plugin.RepositoryActions;
using RepoM.Plugin.Clipboard.ActionProvider;
using SimpleInjector;
using TextCopy;

[UsedImplicitly]
public class ClipboardPackage : IPackage
{
    public string Name => "ClipboardPackage";

    public Task RegisterServicesAsync(Container container, IPackageConfiguration packageConfiguration)
    {
        RegisterPluginHooks(container);
        RegisterInternals(container);
        return Task.CompletedTask;
    }

    private static void RegisterPluginHooks(Container container)
    {
        // repository actions
        container.RegisterDefaultRepositoryActionDeserializerForType<RepositoryActionClipboardCopyV1>();
        container.Collection.Append<IActionToRepositoryActionMapper, ActionClipboardCopyV1Mapper>(Lifestyle.Singleton);

        // ordering
        // (see Statistics for example)

        // action executor
        container.Register(typeof(ICommandExecutor<>), new[] { typeof(ClipboardPackage).Assembly, }, Lifestyle.Singleton);

        // variable provider

        // module
    }

    private static void RegisterInternals(Container container)
    {
        container.RegisterSingleton<IClipboard, Clipboard>();
    }
}