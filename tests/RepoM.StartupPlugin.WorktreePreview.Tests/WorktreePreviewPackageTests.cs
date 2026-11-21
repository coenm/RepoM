namespace RepoM.StartupPlugin.WorktreePreview.Tests;

using System.Threading.Tasks;
using AwesomeAssertions;
using FakeItEasy;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using RepoM.Api;
using RepoM.Core.Plugin;
using RepoM.Core.Plugin.Common;
using RepoM.Core.Plugin.Repository;
using RepoM.Core.Plugin.RepositoryActions;
using RepoM.Core.Plugin.RepositoryActions.Commands;
using RepoM.Core.Repositories;
using RepoM.Core.Repositories.Favorite;
using RepoM.Core.Repositories.Monitoring;
using RepoM.Core.Repositories.Store;
using RepoM.StartupPlugin.WorktreePreview;
using RepoM.UI;
using SimpleInjector;
using Xunit;

public class WorktreePreviewPackageTests
{
    [WpfFact]
    public async Task Package_ShouldReplaceOnlyTheApplicationWindow_AndUseDemoServices()
    {
        using var store = new RepositoryStore();
        using var container = CreateContainer(store);
        container.RegisterInstance(A.Fake<IApplicationWindow>());

        await new WorktreePreviewPackage().RegisterServicesAsync(container, A.Fake<IPackageConfiguration>());
        var window = container.GetInstance<IApplicationWindow>();
        try
        {
            window.Window.Should().BeOfType<WorktreePreviewWindow>();
            container.GetInstance<IApplicationPresentation>().UseTray.Should().BeFalse();
            await container.GetInstance<IRepositoryMonitorService>().StartAsync();
            store.Count.Should().Be(5);
            Fake.GetCalls(container.GetInstance<ProductionServices>().Monitor).Should().BeEmpty();
        }
        finally
        {
            window.Shutdown();
        }
    }

    private static Container CreateContainer(RepositoryStore store)
    {
        var container = new Container();
        var production = new ProductionServices();
        container.RegisterInstance(production);
        container.RegisterInstance(production.Monitor);
        container.RegisterInstance(production.Favorites);
        container.RegisterInstance(production.GitExecutor);
        container.RegisterInstance(production.ProcessExecutor);
        container.RegisterInstance<IRepositoryStore>(store);
        container.RegisterInstance<IRepositoryMonitoringService>(new RepositoryMonitoringStateService());
        container.RegisterInstance<IApplicationPresentation>(new DefaultPresentation());
        container.RegisterInstance<ILogger>(NullLogger.Instance);
        container.Collection.Append<IModule>(() => container.GetInstance<IRepositoryMonitorService>(), Lifestyle.Singleton);
        return container;
    }

    private sealed class ProductionServices
    {
        public IRepositoryMonitorService Monitor { get; } = A.Fake<IRepositoryMonitorService>();
        public IFavoriteStore Favorites { get; } = A.Fake<IFavoriteStore>();
        public ICommandExecutor<GitRepositoryCommand> GitExecutor { get; } = A.Fake<ICommandExecutor<GitRepositoryCommand>>();
        public ICommandExecutor<StartProcessRepositoryCommand> ProcessExecutor { get; } = A.Fake<ICommandExecutor<StartProcessRepositoryCommand>>();
    }

    private sealed class DefaultPresentation : IApplicationPresentation
    {
        public string WindowTitle => "RepoM";

        public bool UseTray => true;
    }
}
