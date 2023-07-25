namespace RepoM.Plugin.WindowsExplorerGitInfo;

using System.Threading.Tasks;
using JetBrains.Annotations;
using RepoM.Core.Plugin;
using RepoM.Plugin.WindowsExplorerGitInfo.PInvoke.Explorer;
using SimpleInjector;

[UsedImplicitly]
public class WindowsExplorerGitInfoPackage : IPackageWithConfiguration
{
    public string Name => "WindowsExplorerGitInfoPackage";

    public Task RegisterServicesAsync(Container container, IPackageConfiguration packageConfiguration)
    {
        container.Register<IWindowsExplorerHandler, WindowsExplorerHandler>(Lifestyle.Singleton);
        container.Collection.Append<IModule, WindowExplorerBarGitInfoModule>(Lifestyle.Singleton);
        return Task.CompletedTask;
    }
}