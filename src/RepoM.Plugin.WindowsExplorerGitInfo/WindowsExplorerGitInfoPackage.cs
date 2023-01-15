namespace RepoM.Plugin.WindowsExplorerGitInfo;

using JetBrains.Annotations;
using RepoM.Core.Plugin;
using RepoM.Plugin.WindowsExplorerGitInfo.PInvoke.Explorer;
using SimpleInjector;
using SimpleInjector.Packaging;

[UsedImplicitly]
public class WindowsExplorerGitInfoPackage : IPackage
{
    public void RegisterServices(Container container)
    {
        container.Register<IWindowsExplorerHandler, WindowsExplorerHandler>(Lifestyle.Singleton);
        container.Collection.Append<IModule, WindowExplorerBarGitInfoModule>(Lifestyle.Singleton);
    }
}