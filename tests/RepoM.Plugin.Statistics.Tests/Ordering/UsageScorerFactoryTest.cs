namespace RepoM.Plugin.Statistics.Tests.Ordering;

using FakeItEasy;
using FluentAssertions;
using RepoM.Core.Plugin.Common;
using RepoM.Core.Plugin.RepositoryOrdering;
using RepoM.Plugin.Statistics.Ordering;
using Xunit;

public class UsageScorerFactoryTest
{
    private readonly StatisticsService _service;
    private readonly IClock _clock;

    public UsageScorerFactoryTest()
    {
        _clock = A.Fake<IClock>();
        _service = new StatisticsService(_clock);
    }

    [Fact]
    public void Create_ShouldReturnInstanceOfUsageScoreCalculator()
    {
        // arrange
        var sut = new UsageScorerFactory(_service, _clock);
        var config = new UsageScorerConfigurationV1();

        // act
        IRepositoryScoreCalculator result = sut.Create(config);

        // assert
        result.Should().BeOfType<UsageScoreCalculator>();
    }
}