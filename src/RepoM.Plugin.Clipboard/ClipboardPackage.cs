namespace RepoM.Plugin.Clipboard;

using System.Threading.Tasks;
using JetBrains.Annotations;
using RepoM.Api.IO.ModuleBasedRepositoryActionProvider.Data;
using RepoM.Core.Plugin;
using RepoM.Core.Plugin.RepositoryActions;
using RepoM.Plugin.Clipboard.RepositoryAction;
using RepoM.Plugin.Clipboard.RepositoryAction.Actions;
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
        // new style
        container.RegisterActionMenuType<ActionMenu.Model.ActionMenus.ClipboardCopy.RepositoryActionClipboardCopyV1>();
        container.RegisterActionMenuMapper<ActionMenu.Model.ActionMenus.ClipboardCopy.RepositoryActionClipboardCopyV1Mapper>(Lifestyle.Singleton);

        // ordering
        // (see Statistics for example)

        // action executor
        container.Register<ICommandExecutor<CopyToClipboardRepositoryCommand>, CopyToClipboardRepositoryCommandExecutor>(Lifestyle.Singleton);

        // variable provider

        // module
    }

    private static void RegisterInternals(Container container)
    {
        container.RegisterSingleton<IClipboard, Clipboard>();
    }
}