namespace RepoM.Plugin.Statistics.Tests.Ordering;

using System;
using FakeItEasy;
using FluentAssertions;
using RepoM.Core.Plugin.Common;
using RepoM.Core.Plugin.RepositoryOrdering;
using RepoM.Plugin.Statistics.Ordering;
using Xunit;

public class UsageScorerFactoryTest
{
    private readonly IStatisticsService _service;
    private readonly IClock _clock;

    public UsageScorerFactoryTest()
    {
        _clock = A.Fake<IClock>();
        _service = new StatisticsService(_clock);
    }

    [Fact]
    public void Ctor_ShouldThrow_WhenArgumentNull()
    {
        // arrange

        // act
        Func<UsageScorerFactory> act1 = () => new UsageScorerFactory(null!, A.Dummy<IClock>());
        Func<UsageScorerFactory> act2 = () => new UsageScorerFactory(_service, null!);

        // assert
        act1.Should().Throw<ArgumentNullException>();
        act2.Should().Throw<ArgumentNullException>();
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