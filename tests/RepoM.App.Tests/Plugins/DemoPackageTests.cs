namespace RepoM.App.Tests.Plugins;

using System;
using System.IO;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AwesomeAssertions;
using FakeItEasy;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using RepoM.ActionMenu.Core;
using RepoM.ActionMenu.Interface.UserInterface;
using RepoM.Api.IO;
using RepoM.Api;
using RepoM.Api.Common;
using RepoM.Api.Plugins;
using RepoM.UI.RepositoryActions;
using RepoM.UI.Services;
using RepoM.Core.Plugin;
using RepoM.Core.Plugin.Common;
using RepoM.Core.Plugin.Repository;
using RepoM.Core.Plugin.RepositoryActions;
using RepoM.Core.Plugin.RepositoryActions.Commands;
using RepoM.Core.Repositories;
using RepoM.Core.Repositories.Adapters;
using RepoM.Core.Repositories.Favorite;
using RepoM.Core.Repositories.Monitoring;
using RepoM.Core.Repositories.Store;
using RepoM.StartupPlugin.Demo;
using RepoM.StartupPlugin.WorktreePreview;
using RepoM.UI;
using SimpleInjector;
using Xunit;

public class DemoPackageTests
{
    [Fact]
    public async Task DemoProfile_ShouldInstallAndResetAllSampleFiles()
    {
        var files = new MockFileSystem();
        var paths = new AppDataPathProvider(new AppDataPathConfig { AppSettingsPath = @"C:\DemoSettings", }, files);
        var filenames = new[] { "RepositoryActionsV2.yaml", "RepoM.Filtering.yaml", "RepoM.Ordering.yaml", };

        await DemoProfile.PrepareAsync(files, paths);
        var contents = filenames.ToDictionary(
            filename => filename,
            filename => files.File.ReadAllText(files.Path.Combine(paths.AppDataPath, filename)));
        foreach (var filename in filenames)
        {
            contents[filename].Should().NotBeNullOrWhiteSpace();
            await files.File.WriteAllTextAsync(files.Path.Combine(paths.AppDataPath, filename), "modified");
        }

        await DemoProfile.PrepareAsync(files, paths);

        foreach (var filename in filenames)
        {
            (await files.File.ReadAllTextAsync(files.Path.Combine(paths.AppDataPath, filename))).Should().Be(contents[filename]);
        }
    }

    [WpfFact]
    public async Task PreviewPackage_ShouldReplaceOnlyTheApplicationWindow_AndUseDemoServices()
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

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task StartupPackage_ShouldBeLoadedWhenRequired_EvenIfDisabledInProfile(bool required)
    {
        var files = new MockFileSystem();
        var paths = new AppDataPathProvider(new AppDataPathConfig { AppSettingsPath = @"C:\DemoSettings" }, files);
        await DemoProfile.PrepareAsync(files, paths);
        var plugin = new PluginInfo(typeof(DemoPackage).Assembly.Location, null, [], isRequired: required);
        var settings = new FileAppSettingsService(paths, files, NullLogger.Instance)
        {
            Plugins = [new PluginSettings(plugin.Name, plugin.AssemblyPath, false)],
        };
        var finder = A.Fake<IPluginFinder>();
        A.CallTo(() => finder.FindPlugins(A<string>._)).Returns([plugin]);
        using var store = new RepositoryStore();
        using var container = CreateContainer(store);
        var bootstrapper = new CoreBootstrapper(finder, files, paths, NullLoggerFactory.Instance);

        await bootstrapper.LoadAndRegisterPluginsAsync(container, @"C:\App");

        container.GetInstance<IApplicationPresentation>().UseTray.Should().Be(!required);
        settings.Plugins.Should().ContainSingle().Which.Enabled.Should().BeFalse();
    }

    [Fact]
    public async Task Package_ShouldKeepLifecycleAndRefreshInsideDemo_AndRepopulateAfterClear()
    {
        using var store = new RepositoryStore();
        using var container = CreateContainer(store);
        await new DemoPackage().RegisterServicesAsync(container, A.Fake<IPackageConfiguration>());
        var realMonitor = container.GetInstance<ProductionServices>().Monitor;

        var monitor = container.GetInstance<IRepositoryMonitorService>();
        var module = container.GetAllInstances<IModule>().Single();
        module.Should().BeSameAs(monitor);
        await module.StartAsync();
        var initial = store.Items.OrderBy(repository => repository.Name).ToArray();
        initial.Should().HaveCount(5);
        initial.Should().Contain(repository => !repository.WasFound);
        initial.Should().Contain(repository => repository.HasLocalChanges && repository.IsBehind);
        var primary = initial.Single(repository => repository.Name == "RepoM");

        monitor.RemoveStaleRepositories();
        await monitor.RefreshAllAsync();
        (await monitor.RefreshRepositoryAsync(primary.Path)).Should().BeSameAs(primary);
        store.Count.Should().Be(5);
        monitor.DeactivateMonitoring(primary.SafePath);
        container.GetInstance<IRepositoryMonitoringService>().IsMonitored(primary.SafePath).Should().BeFalse();
        monitor.ActivateMonitoring(primary.SafePath);
        container.GetInstance<IRepositoryMonitoringService>().IsMonitored(primary.SafePath).Should().BeTrue();

        store.Clear();
        monitor.CancelAllScans();
        await monitor.ScanAsync();
        store.Items.OrderBy(repository => repository.Name).Should().BeEquivalentTo(initial);
        await module.StopAsync();
        monitor.Dispose();

        Fake.GetCalls(realMonitor).Should().BeEmpty("the real monitor must never start, refresh, persist or dispose");
    }

    [Fact]
    public async Task Package_ShouldSimulateActionsAndKeepFavoritesInMemory()
    {
        using var store = new RepositoryStore();
        using var container = CreateContainer(store);
        await new DemoPackage().RegisterServicesAsync(container, A.Fake<IPackageConfiguration>());
        var production = container.GetInstance<ProductionServices>();
        var favorites = container.GetInstance<IFavoriteStore>();
        favorites.Load().Should().ContainSingle().Which.Should().Be("C:/RepoM-Demo/RepoM");
        favorites.Save(["C:/RepoM-Demo/WebShop"]);
        favorites.Load().Should().Equal("C:/RepoM-Demo/WebShop");

        var presentation = container.GetInstance<IApplicationPresentation>();
        presentation.UseTray.Should().BeFalse();
        presentation.WindowTitle.Should().Be("RepoM - Demo");

        var executor = new ActionExecutor(container, NullLogger.Instance);
        executor.Execute(A.Fake<IRepository>(), GitRepositoryCommand.Fetch).Should().BeTrue();
        Fake.GetCalls(production.GitExecutor).Should().BeEmpty();
        Fake.GetCalls(production.Favorites).Should().BeEmpty();
    }

    [Fact]
    public async Task DemoMenu_ShouldUseRealTemplatesAndDeferredBranchMenus()
    {
        var directory = Directory.CreateTempSubdirectory("RepoM-demo-tests-");
        try
        {
            using var store = new RepositoryStore();
            using var container = CreateContainer(store);
            // The action-menu cache watches profile files through HostFileChangeMonitor.
            var files = new FileSystem();
            var paths = new AppDataPathProvider(new AppDataPathConfig { AppSettingsPath = directory.FullName }, files);
            await DemoProfile.PrepareAsync(files, paths);
            container.RegisterInstance<IFileSystem>(files);
            RepoM.ActionMenu.Core.Bootstrapper.RegisterServices(container);
            await new DemoPackage().RegisterServicesAsync(container, A.Fake<IPackageConfiguration>());
            var monitor = container.GetInstance<IRepositoryMonitorService>();
            await monitor.StartAsync();
            var info = store.Items.Single(repository => repository.Name == "RepoM");
            var repository = new RepositoryInfoAdapter(info);
            var factory = new UserMenuActionMenuFactory(files, paths, container.GetInstance<IUserInterfaceActionMenuFactory>());

            var actions = await factory.CreateMenuAsync(repository).ToListAsync();

            actions.OfType<UserInterfaceRepositoryAction>().First().Name.Should().Be("RepoM - feature/demo");
            var git = actions.OfType<UserInterfaceRepositoryAction>().Single(action => action.Name == "Git");
            var checkout = git.SubActions!.OfType<DeferredSubActionsUserInterfaceRepositoryAction>().Single();
            var branches = await checkout.GetAsync();
            branches.OfType<UserInterfaceRepositoryAction>().Should().Contain(action => action.Name == "main" && action.CanExecute);
            branches.OfType<UserInterfaceRepositoryAction>().Should().Contain(action => action.Name == "feature/demo" && !action.CanExecute);

            var command = actions.OfType<UserInterfaceRepositoryAction>()
                .First(action => action.RepositoryCommand is StartProcessRepositoryCommand).RepositoryCommand;
            new ActionExecutor(container, NullLogger.Instance).Execute(repository, command).Should().BeTrue();
            Fake.GetCalls(container.GetInstance<ProductionServices>().ProcessExecutor).Should().BeEmpty();
        }
        finally
        {
            directory.Delete(recursive: true);
        }
    }

    [Fact]
    public void Discovery_ShouldSelectOnlyDemoWithoutDiscoveringExternalPlugins()
    {
        var finder = A.Fake<IPluginFinder>();
        A.CallTo(() => finder.FindPlugins(@"C:\App")).Returns(
        [
            new PluginInfo(@"C:\App\RepoM.Plugin.Demo.dll", null, []),
            new PluginInfo(@"C:\App\RepoM.Plugin.Statistics.dll", null, []),
        ]);
        var files = new MockFileSystem();
        files.AddFile(typeof(DemoPackage).Assembly.Location, new MockFileData("plugin"));
        var sut = new DemoPluginFinderDecorator(finder, files, new HmacSha256Service());

        var result = sut.FindPlugins(@"C:\App").ToArray();

        result.Should().ContainSingle().Which.AssemblyPath.Should().Be(typeof(DemoPackage).Assembly.Location);
        A.CallTo(() => finder.FindPlugins(A<string>._)).MustNotHaveHappened();
    }

    [Fact]
    public async Task CancelledScan_ShouldNotPopulateStore()
    {
        using var store = new RepositoryStore();
        var monitor = new DemoRepositoryMonitorDecorator(() => throw new InvalidOperationException(),
            store, new RepositoryMonitoringStateService());

        Func<Task> scan = () => monitor.ScanAsync(new CancellationToken(true));

        await scan.Should().ThrowAsync<OperationCanceledException>();
        store.Count.Should().Be(0);
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
        container.RegisterInstance<IApplicationPresentation>(new ApplicationPresentation());
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
}
