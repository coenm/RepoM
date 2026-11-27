namespace RepoM.App.Tests;

using System;
using System.IO;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using System.Threading.Tasks;
using AwesomeAssertions;
using RepoM.Api;
using RepoM.Api.IO;
using RepoM.Api.Plugins;
using RepoM.App.Startup;
using RepoM.Core.StartupPlugin;
using RepoM.StartupPlugin.Demo;
using RepoM.StartupPlugin.WorktreePreview;
using SimpleInjector;
using Xunit;

public class StartupTests
{
    [Theory]
    [InlineData("--startup-plugin", @"C:\Plugins with spaces\demo.dll")]
    [InlineData("--startup-plugin=C:\\Plugins with spaces\\demo.dll")]
    public void Arguments_ShouldConsumeOnlyPluginSelection(params string[] selection)
    {
        var result = StartupArguments.Parse(
            [.. selection, "--App:AppSettingsPath", @"C:\Profile"]);

        result.PluginPath.Should().Be(@"C:\Plugins with spaces\demo.dll");
        result.ConfigurationArguments.Should().Equal("--App:AppSettingsPath", @"C:\Profile");
    }

    [Theory]
    [InlineData("--startup-plugin")]
    [InlineData("--startup-plugin=")]
    [InlineData("--startup-plugin", "--App:AppSettingsPath")]
    [InlineData("--startup-plugin=a.dll", "--startup-plugin=b.dll")]
    [InlineData("--demo")]
    [InlineData("--worktree-preview")]
    [InlineData("--preview-worktree")]
    public void Arguments_ShouldRejectInvalidSelection(params string[] args)
    {
        Action parse = () => StartupArguments.Parse(args);
        parse.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Loader_ShouldLoadPluginByPath()
    {
        StartupPluginLoader.Load(typeof(DemoStartupPlugin).Assembly.Location)
            .Should().BeOfType<DemoStartupPlugin>();
        StartupPluginLoader.Load(typeof(WorktreePreviewStartupPlugin).Assembly.Location)
            .Should().BeOfType<WorktreePreviewStartupPlugin>();
    }

    [Fact]
    public void Loader_ShouldReportMissingOrInvalidPlugin()
    {
        Action missing = () => StartupPluginLoader.Load(Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".dll"));
        Action noContract = () => StartupPluginLoader.Load(typeof(IStartupPlugin).Assembly.Location);
        Action notDll = () => StartupPluginLoader.Load("plugin.txt");

        missing.Should().Throw<FileNotFoundException>();
        noContract.Should().Throw<InvalidOperationException>().WithMessage("*exactly one*");
        notDll.Should().Throw<ArgumentException>();
    }

    [Fact]
    public async Task DefaultStartup_ShouldUseSeparateContainersAndShareFinalInstances()
    {
        using var startup = await StartupDependencies.CreateAsync(new StartupArguments(null, []));
        using var application = new Container();
        startup.RegisterInstances(application);
        application.RegisterSingleton<ApplicationOnlyService>();
        application.Verify();

        startup.FileSystem.Should().BeOfType<FileSystem>();
        startup.HmacService.Should().BeOfType<HmacSha256Service>();
        startup.PluginFinder.Should().BeOfType<PluginFinder>();
        startup.PathProviderFactory.Should().BeOfType<ConfigBasedAppDataPathProviderFactory>();
        application.GetInstance<IFileSystem>().Should().BeSameAs(startup.FileSystem);
        application.GetInstance<IHmacService>().Should().BeSameAs(startup.HmacService);
        application.GetInstance<IPluginFinder>().Should().BeSameAs(startup.PluginFinder);
        application.GetInstance<IConfigBasedAppDataPathProviderFactory>().Should().BeSameAs(startup.PathProviderFactory);
    }

    [Fact]
    public async Task StartupPlugin_ShouldReplaceAndDecorateDefaultsBeforeResolution_AndRetainOwnership()
    {
        var startup = await StartupDependencies.CreateAsync(
            new StartupArguments(typeof(TestStartupPlugin).Assembly.Location, []));
        var hmac = startup.HmacService.Should().BeOfType<TrackingHmacDecorator>().Subject;
        try
        {
            using (var application = new Container())
            {
                startup.RegisterInstances(application);
                application.Verify();
                application.GetInstance<IFileSystem>().Should().BeOfType<MockFileSystem>();
                application.GetInstance<IHmacService>().Should().BeSameAs(hmac);
                application.GetInstance<IPluginFinder>().Should().BeOfType<DemoPluginFinderDecorator>();
                var paths = await application.GetInstance<IConfigBasedAppDataPathProviderFactory>().CreateAsync();
                paths.AppDataPath.Should().EndWith(@"RepoM\Demo");
                startup.FileSystem.File.Exists(
                    startup.FileSystem.Path.Combine(paths.AppDataPath, "RepositoryActionsV2.yaml")).Should().BeTrue();
            }

            hmac.DisposeCount.Should().Be(0);
        }
        finally
        {
            startup.Dispose();
        }

        hmac.DisposeCount.Should().Be(1);
    }

    [Fact]
    public async Task Defaults_ShouldUseReplacedFileSystemForPathFactory()
    {
        using var container = new Container();
        await CoreBootstrapper.RegisterStartupDependenciesAsync(container, ["--App:AppSettingsPath", "Profile"]);
        var files = new MockFileSystem();
        files.Directory.CreateDirectory(@"C:\MockRoot");
        files.Directory.SetCurrentDirectory(@"C:\MockRoot");
        container.Options.AllowOverridingRegistrations = true;
        container.RegisterInstance<IFileSystem>(files);
        container.Verify();

        var paths = await container.GetInstance<IConfigBasedAppDataPathProviderFactory>().CreateAsync();
        paths.AppDataPath.Should().Be(@"C:\MockRoot\Profile");
    }

    [Fact]
    public void NormalDiscovery_ShouldNotSelectStartupAssemblies()
    {
        var files = new MockFileSystem();
        files.AddFile(@"C:\App\RepoM.StartupPlugin.Demo.dll", new MockFileData("not a normal plugin"));
        files.AddFile(@"C:\App\RepoM.StartupPlugin.WorktreePreview.dll", new MockFileData("not a normal plugin"));
        var finder = new PluginFinder(files, new HmacSha256Service());

        finder.FindPlugins(@"C:\App").Should().BeEmpty();
    }

    public sealed class ApplicationOnlyService
    {
    }

    public sealed class TestStartupPlugin : IStartupPlugin
    {
        public async Task RegisterStartupDependenciesAsync(Container container)
        {
            container.Register<IFileSystem>(() => new MockFileSystem(), Lifestyle.Singleton);
            container.RegisterDecorator<IHmacService, TrackingHmacDecorator>(Lifestyle.Singleton);
            await new DemoStartupPlugin().RegisterStartupDependenciesAsync(container);
            await Task.Yield();
        }
    }

    public sealed class TrackingHmacDecorator(IHmacService decoratee) : IHmacService, IDisposable
    {
        public int DisposeCount { get; private set; }

        public byte[] GetHmac(Stream stream) => decoratee.GetHmac(stream);

        public bool ValidateHmac(Stream input, in byte[] hmac) => decoratee.ValidateHmac(input, in hmac);

        public void Dispose() => DisposeCount++;
    }
}
