namespace RepoM.Plugin.Demo;

using System;
using System.Threading.Tasks;
using RepoM.Core.Plugin;
using RepoM.Core.Plugin.Common;
using RepoM.Core.Plugin.RepositoryActions;
using RepoM.Core.Repositories;
using RepoM.Core.Repositories.Favorite;
using SimpleInjector;

/// <summary>
/// Registers demo application decorators after the startup plugin selects this package.
/// </summary>
public sealed class DemoPackage : IPackage
{
    public string Name => "DemoPackage";

    public Task RegisterServicesAsync(Container container, IPackageConfiguration packageConfiguration)
    {
        ArgumentNullException.ThrowIfNull(container);
        ArgumentNullException.ThrowIfNull(packageConfiguration);
        container.RegisterDecorator<IRepositoryMonitorService, DemoRepositoryMonitorDecorator>(Lifestyle.Singleton);
        container.RegisterDecorator<IFavoriteStore, DemoFavoriteStoreDecorator>(Lifestyle.Singleton);
        container.RegisterDecorator<IApplicationPresentation, DemoPresentationDecorator>(Lifestyle.Singleton);
        container.RegisterDecorator(typeof(ICommandExecutor<>), typeof(DemoCommandExecutorDecorator<>), Lifestyle.Singleton);
        return Task.CompletedTask;
    }
}
