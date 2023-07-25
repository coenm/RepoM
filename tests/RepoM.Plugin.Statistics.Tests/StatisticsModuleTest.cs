namespace RepoM.Plugin.Statistics.Tests;

using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using System.Threading.Tasks;
using FakeItEasy;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using RepoM.Core.Plugin.Common;
using RepoM.Core.Plugin.Repository;
using Xunit;
using IClock = Core.Plugin.Common.IClock;

public class StatisticsModuleTest
{
    private readonly IClock _clock;
    private readonly IAppDataPathProvider _pathProvider;
    private readonly ILogger _logger;
    private readonly IStatisticsConfiguration _configuration;
    private readonly DateTime _now;

    public StatisticsModuleTest()
    {
        _now = new DateTime(2023, 04, 29, 23, 12, 55, DateTimeKind.Utc);
        _clock = A.Fake<IClock>();
        A.CallTo(() => _clock.Now).Returns(_now);
        _pathProvider = A.Fake<IAppDataPathProvider>();
        A.CallTo(() => _pathProvider.GetAppDataPath()).Returns("C:\\data");
        _logger = A.Fake<ILogger>();
        _configuration = A.Fake<IStatisticsConfiguration>();
    }

    [Fact]
    public void Ctor_ShouldThrow_WhenArgumentNull()
    {
        // arrange
        IStatisticsService statisticsService = A.Dummy<IStatisticsService>();
        IStatisticsConfiguration configuration = A.Dummy<IStatisticsConfiguration>();
        IClock clock = A.Dummy<IClock>();
        IAppDataPathProvider pathProvider = A.Dummy<IAppDataPathProvider>();
        IFileSystem fileSystem = A.Dummy<IFileSystem>();
        ILogger logger = A.Dummy<ILogger>();

        // act
        Func<StatisticsModule> act1 = () => new StatisticsModule(statisticsService, configuration, clock, pathProvider, fileSystem, null!);
        Func<StatisticsModule> act2 = () => new StatisticsModule(statisticsService, configuration, clock, pathProvider, null!, logger);
        Func<StatisticsModule> act3 = () => new StatisticsModule(statisticsService, configuration, clock, null!, fileSystem, logger);
        Func<StatisticsModule> act4 = () => new StatisticsModule(statisticsService, configuration, null!, pathProvider, fileSystem, logger);
        Func<StatisticsModule> act5 = () => new StatisticsModule(statisticsService, null!, clock, pathProvider, fileSystem, logger);
        Func<StatisticsModule> act6 = () => new StatisticsModule(null!, configuration, clock, pathProvider, fileSystem, logger);

        // assert
        act1.Should().Throw<ArgumentNullException>();
        act2.Should().Throw<ArgumentNullException>();
        act3.Should().Throw<ArgumentNullException>();
        act4.Should().Throw<ArgumentNullException>();
        act5.Should().Throw<ArgumentNullException>();
        act6.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public async Task StartAsync_ShouldInitialize()
    {
        // arrange
        IFileSystem fileSystem = new MockFileSystem();
        var statisticsService = new StatisticsService(_clock);
        var sut = new StatisticsModule(statisticsService, _configuration, _clock, _pathProvider, fileSystem, _logger);

        // act
        await sut.StartAsync();

        // assert
        statisticsService.GetRepositories().Should().BeEmpty();
    }

    // long running
    [Fact]
    public async Task StopAsync_ShouldStopWritingEvents()
    {
        // arrange
        IRepository repository = A.Fake<IRepository>();
        A.CallTo(() => repository.SafePath).Returns("C:\\tmp-test");
        A.CallTo(() => _configuration.PersistenceBuffer).Returns(TimeSpan.FromSeconds(10)); // minimal
        IFileSystem fileSystem = A.Fake<IFileSystem>();
        var statisticsService = new StatisticsService(_clock);
        var sut = new StatisticsModule(statisticsService, _configuration, _clock, _pathProvider, fileSystem, _logger);
        await sut.StartAsync();

        // act
        await sut.StopAsync();
        await Task.Delay(TimeSpan.FromSeconds(2)); // not needed but to be more robus against race condition.
        Fake.ClearRecordedCalls(fileSystem);
        statisticsService.Record(repository);
        await Task.Delay(TimeSpan.FromSeconds(10));
        await Task.Delay(TimeSpan.FromSeconds(5)); // extra
        
        // assert
        A.CallTo(fileSystem).MustNotHaveHappened();
    }

    [Fact]
    public async Task StartAsync_ShouldProcessEventsFromFiles()
    {
        // arrange
        IRepository repository = A.Fake<IRepository>();
        A.CallTo(() => repository.SafePath).Returns("C:\\tmp-test");
        A.CallTo(() => _configuration.PersistenceBuffer).Returns(TimeSpan.FromSeconds(10)); // minimal
        IFileSystem fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>()
            {
                { "C:\\data\\Module\\Statistics\\statistics.v1.1.json", new MockFileData("invalid json") },
                { "C:\\data\\Module\\Statistics\\statistics.v1.2.json", new MockFileData("{\"$type\":\"RepoM.Plugin.Statistics.Interface.IEvent[], RepoM.Plugin.Statistics\",\"$values\":[{\"$type\":\"RepoM.Plugin.Statistics.Interface.RepositoryActionRecordedEvent, RepoM.Plugin.Statistics\",\"Repository\":\"C:/Projects/git/E-Accounting/E-Accounting\",\"Timestamp\":\"2023-06-19T10:46:25.2014875+02:00\"}]}") },
            });
        var statisticsService = new StatisticsService(_clock);
        var sut = new StatisticsModule(statisticsService, _configuration, _clock, _pathProvider, fileSystem, _logger);

        // act
        await sut.StartAsync();

        // assert
        statisticsService.GetRepositories().Should().BeEquivalentTo(new[] { "C:/Projects/git/E-Accounting/E-Accounting", });
    }
}