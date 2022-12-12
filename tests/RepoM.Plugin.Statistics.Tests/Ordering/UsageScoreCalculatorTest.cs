namespace RepoM.Plugin.Statistics.Tests.Ordering;

using System;
using System.Collections.Generic;
using FakeItEasy;
using FluentAssertions;
using RepoM.Core.Plugin.Repository;
using RepoM.Plugin.Statistics.Ordering;
using Xunit;
using IClock = RepoM.Core.Plugin.Common.IClock;

public class UsageScoreCalculatorTest
{
    private readonly StatisticsService _service;
    private readonly IClock _calculatorClock;
    private readonly IRepository _repository;
    private readonly ScoreCalculatorConfig _defaultConfig;
    private UsageScoreCalculator _sut;

    public UsageScoreCalculatorTest()
    {
        _repository =  A.Fake<IRepository>();
        A.CallTo(() => _repository.SafePath).Returns("DummyValue");

        _calculatorClock = A.Fake<IClock>();
        IClock statisticsServiceClock = A.Fake<IClock>();
        _service = new StatisticsService(statisticsServiceClock);

        var now = new DateTime(2022, 2, 3, 5, 6, 7, 8);
        A.CallTo(() => _calculatorClock.Now).Returns(now);

        _defaultConfig = new ScoreCalculatorConfig
            {
                MaxScore = 100,
                Ranges = new List<RangeConfig>
                    {
                        new()
                            {
                                Score = 3,
                                MaxItems = 100,
                                MaxAge = new TimeSpan(0, 2, 0, 0),
                            },
                        new()
                            {
                                Score = 1,
                                MaxItems = 100,
                                MaxAge = new TimeSpan(0, 7, 0, 0),
                            },
                    },
            };

        A.CallTo(() => statisticsServiceClock.Now).ReturnsNextFromSequence(
            now.AddHours(-1),
            now.AddHours(-2),
            now.AddHours(-3));

        _service.Record(_repository); // now - 1h
        _service.Record(_repository); // now - 2h
        _service.Record(_repository); // now - 3h

        _sut = new UsageScoreCalculator(_service, _calculatorClock, _defaultConfig);
    }

    [Fact]
    public void Score_ShouldUseRecordings_WhenCalculating()
    {
        // arrange

        // act
        var result = _sut.Score(_repository);
        
        // assert
        result.Should().Be(7);
    }

    [Fact]
    public void Score_MaxCount_Scenario1()
    {
        // arrange
        _defaultConfig.MaxScore = 0;

        // act
        var result = _sut.Score(_repository);

        // assert
        result.Should().Be(0);
    }

    [Fact]
    public void Score_MaxCount_Scenario2()
    {
        // arrange
        _defaultConfig.MaxScore = 4;

        // act
        var result = _sut.Score(_repository);

        // assert
        result.Should().Be(4);
    }
    
    [Fact]
    public void Score_ShouldReturnZero_WhenNoRangesSpecified()
    {
        // arrange
        _defaultConfig.Ranges.Clear();
        _sut = new UsageScoreCalculator(_service, _calculatorClock, _defaultConfig);

        // act
        var result = _sut.Score(_repository);

        // assert
        result.Should().Be(0);
    }

    [Fact]
    public void Score_ShouldReturnZero_RepositoryWasNotFound()
    {
        // arrange
        IRepository r = A.Fake<IRepository>();
        A.CallTo(() => r.SafePath).Returns("OtherDummyValue");

        // act
        var result = _sut.Score(r);

        // assert
        result.Should().Be(0);
    }
    
    [Fact]
    public void Score_Scenario1()
    {
        // arrange
        _defaultConfig.Ranges[0].MaxItems = 1;

        // act
        var result = _sut.Score(_repository);

        // assert
        result.Should().Be(5);
    }

    [Fact]
    public void Score_Scenario2()
    {
        // arrange
        _defaultConfig.Ranges[1].MaxItems = 0;

        // act
        var result = _sut.Score(_repository);

        // assert
        result.Should().Be(6);
    }
}