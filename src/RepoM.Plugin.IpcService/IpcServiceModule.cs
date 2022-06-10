namespace RepoM.Plugin.IpcService;

using JetBrains.Annotations;
using RepoM.Ipc;
using RepoZ.Api;
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