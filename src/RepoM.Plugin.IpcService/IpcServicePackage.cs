namespace RepoM.Plugin.IpcService;

using JetBrains.Annotations;
using RepoM.Core.Plugin;
using RepoM.Ipc;
using SimpleInjector;
using SimpleInjector.Packaging;

[UsedImplicitly]
public class IpcServicePackage : IPackage
{
    public void RegisterServices(Container container)
    {
        //IRepositorySource
        container.Register<IIpcEndpoint, DefaultIpcEndpoint>(Lifestyle.Singleton);
        container.Register<IpcServer>(Lifestyle.Singleton);
        container.Collection.Append<IModule, IpcServerModule>(Lifestyle.Singleton);
    }
}