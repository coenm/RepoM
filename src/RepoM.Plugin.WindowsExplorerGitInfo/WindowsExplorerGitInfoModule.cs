namespace RepoM.Plugin.WindowsExplorerGitInfo;

using JetBrains.Annotations;
using RepoM.Plugin.WindowsExplorerGitInfo.PInvoke.Explorer;
using RepoZ.Api;
using SimpleInjector;
using SimpleInjector.Packaging;

[UsedImplicitly]
public class WindowsExplorerGitInfoModule : IPackage
{
    public void RegisterServices(Container container)
    {
        container.Register<WindowsExplorerHandler>(Lifestyle.Singleton);
        container.Collection.Append<IModule, WindowExplorerBarGitInfoModule>(Lifestyle.Singleton);
    }
}