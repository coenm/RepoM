namespace RepoM.Plugin.IpcService;

using JetBrains.Annotations;
using RepoM.Api;
using RepoM.Ipc;
using SimpleInjector;
using SimpleInjector.Packaging;

[UsedImplicitly]
public class IpcServiceModule : IPackage
{
    public void RegisterServices(Container container)
    {
        //IRepositorySource
        container.Register<IIpcEndpoint, DefaultIpcEndpoint>(Lifestyle.Singleton);
        container.Register<IpcServer>(Lifestyle.Singleton);
        container.Collection.Append<IModule, RepozIpcServerModule>(Lifestyle.Singleton);
    }
}