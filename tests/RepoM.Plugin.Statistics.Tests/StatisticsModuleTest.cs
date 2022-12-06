namespace RepoM.Plugin.Statistics.Tests;

using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using System.Threading.Tasks;
using FakeItEasy;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using RepoM.Core.Plugin.Common;
using Xunit;
using IClock = RepoM.Core.Plugin.Common.IClock;

public class StatisticsModuleTest
{
    private readonly IClock _clock;
    private readonly IAppDataPathProvider _pathProvider;
    private readonly ILogger _logger;

    public StatisticsModuleTest()
    {
        _clock = A.Fake<IClock>();
        _pathProvider = A.Fake<IAppDataPathProvider>();
        A.CallTo(() => _pathProvider.GetAppDataPath()).Returns("C:\\data");
        _logger = A.Fake<ILogger>();
    }

    [Fact]
    public async Task StartAsync_ShouldInitialize()
    {
        // arrange
        IFileSystem fileSystem = new MockFileSystem();
        var statisticsService = new StatisticsService(_clock);
        var sut = new StatisticsModule(statisticsService, _clock, _pathProvider, fileSystem, _logger);

        // act
        await sut.StartAsync();

        // assert
        statisticsService.GetRepositories().Should().BeEmpty();
    }
}