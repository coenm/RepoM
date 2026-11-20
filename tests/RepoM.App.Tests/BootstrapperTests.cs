namespace RepoM.App.Tests;

using System;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FakeItEasy;
using AwesomeAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using RepoM.Core.Plugin;
using RepoM.Core.Plugin.Common;
using RepoM.Core.Repositories;
using RepoM.Core.Repositories.Model;
using RepoM.Core.Repositories.Monitoring;
using RepoM.Core.Repositories.Persistence;
using RepoM.Core.Repositories.Reading;
using RepoM.Core.Repositories.Scanning;
using RepoM.Core.Repositories.Store;
using RepoM.Core.Repositories.Watching;
using SimpleInjector;
using Xunit;
using Sut = RepoM.App.Bootstrapper;

public class BootstrapperTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task RepositoryMonitor_ShouldShareSingletonBetweenConsumersAndModuleLifecycle(bool decorate)
    {
        using var container = new Container();
        container.RegisterInstance(A.Fake<IRepositoryScanner>());
        container.RegisterInstance(A.Fake<IRepositoryWatcher>());
        container.RegisterInstance(A.Fake<IRepositoryInfoReader>());
        container.RegisterInstance<IRepositoryStore>(new RepositoryStore());
        container.RegisterInstance<IFileSystem>(new MockFileSystem());
        container.RegisterInstance<Func<IEnumerable<string>>>(() => []);
        container.RegisterInstance(A.Fake<IRepositoryMonitoringService>());
        container.RegisterInstance(A.Fake<IRepositoryMonitoringEvents>());
        container.RegisterInstance(A.Fake<IRepositorySnapshotStore>());
        container.RegisterInstance(new RepositoryMonitorServiceConfiguration(TimeSpan.FromSeconds(5)));
        container.RegisterInstance<ILogger>(NullLogger.Instance);

        Sut.RegisterRepositoryMonitorServices(container);
        if (decorate)
        {
            // Plugins register decorators after the host has registered the module collection.
            container.RegisterDecorator<IRepositoryMonitorService, TestMonitorDecorator>(Lifestyle.Singleton);
        }

        var module = container.GetAllInstances<IModule>().Single();
        var monitor = container.GetInstance<IRepositoryMonitorService>();
        module.Should().BeSameAs(monitor);
        container.GetInstance<IRepositoryMonitorService>().Should().BeSameAs(monitor);

        if (decorate)
        {
            var decorator = monitor.Should().BeOfType<TestMonitorDecorator>().Subject;
            await module.StartAsync();
            await monitor.RefreshAllAsync();
            await module.StopAsync();
            ((IDisposable)module).Dispose();

            decorator.Calls.Should().Equal("start", "refresh", "stop", "dispose");
        }
        else
        {
            monitor.Should().BeOfType<RepositoryMonitorService>();
        }
    }

    [Fact]
    public void Container_ShouldAlwaysBeSameInstance()
    {
        // arrange

        // act
        var result1 = Sut.Container;
        var result2 = Sut.Container;

        // assert
        result1.Should().NotBeNull().And.Subject.Should().BeSameAs(result2);
    }

    [Fact]
    public void RegisterServices_ShouldNotThrow()
    {
        // arrange

        // act
        Action act = () => Sut.RegisterServices(A.Dummy<IFileSystem>(), A.Dummy<IAppDataPathProvider>());

        // assert
        act.Should().NotThrow();
    }


    [Fact]
    public void RegisterLogging_ShouldNotThrow()
    {
        // arrange

        // act
        Action act = () => Sut.RegisterLogging(A.Dummy<ILoggerFactory>());

        // assert
        act.Should().NotThrow();
    }
    
    [Fact(Skip = "Fails")]
    public void RegisterServices_ShouldResultInValidContainer()
    {
        // arrange
        
        // act
        Sut.RegisterServices(A.Dummy<IFileSystem>(), A.Dummy<IAppDataPathProvider>());
        Sut.RegisterLogging(A.Fake<ILoggerFactory>());
    
        // assert
        Sut.Container.Verify(VerificationOption.VerifyOnly);
    }

    public sealed class TestMonitorDecorator(IRepositoryMonitorService decoratee) : IRepositoryMonitorService
    {
        public List<string> Calls { get; } = [];

        public IRepositoryStore Store => decoratee.Store;

        public IObservable<bool> IsScanning => decoratee.IsScanning;

        public bool IsStalenessCheckRunning => decoratee.IsStalenessCheckRunning;

        public Task StartAsync()
        {
            Calls.Add("start");
            return Task.CompletedTask;
        }

        public Task StopAsync()
        {
            Calls.Add("stop");
            return Task.CompletedTask;
        }

        public Task RefreshAllAsync(CancellationToken ct = default)
        {
            Calls.Add("refresh");
            return Task.CompletedTask;
        }

        public void Dispose() => Calls.Add("dispose");

        public void ActivateMonitoring(string safePath) => decoratee.ActivateMonitoring(safePath);

        public void DeactivateMonitoring(string safePath) => decoratee.DeactivateMonitoring(safePath);

        public void CancelAllScans() => decoratee.CancelAllScans();

        public Task ScanAsync(CancellationToken ct = default) => decoratee.ScanAsync(ct);

        public void RemoveStaleRepositories() => decoratee.RemoveStaleRepositories();

        public Task<RepositoryInfo?> RefreshRepositoryAsync(string repositoryPath, CancellationToken ct = default)
            => decoratee.RefreshRepositoryAsync(repositoryPath, ct);
    }
}