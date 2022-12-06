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
    [Fact]
    public async Task StartAsync_ShouldInitialize()
    {
        // arrange
        IClock clock = A.Fake<IClock>();
        IAppDataPathProvider pathProvider = A.Fake<IAppDataPathProvider>();
        A.CallTo(() => pathProvider.GetAppDataPath()).Returns("C:\\data");
        IFileSystem fileSystem = new MockFileSystem();
        ILogger logger = A.Fake<ILogger>();
        var statisticsService = new StatisticsService(clock);
        var sut = new StatisticsModule(statisticsService, clock, pathProvider, fileSystem, logger);

        // act
        await sut.StartAsync();

        // assert
        statisticsService.GetRepositories().Should().BeEmpty();
    }
}