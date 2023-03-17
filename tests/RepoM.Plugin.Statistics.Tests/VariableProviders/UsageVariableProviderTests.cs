namespace RepoM.Plugin.Statistics.Tests.VariableProviders;

using System;
using FakeItEasy;
using FluentAssertions;
using RepoM.Core.Plugin.Repository;
using RepoM.Plugin.Statistics.VariableProviders;
using Xunit;
using System.Collections.Generic;

public class UsageVariableProviderTests
{
    private readonly UsageVariableProvider _sut;
    private readonly RepositoryContext _repositoryContext;
    private readonly IRepository _repository;
    private readonly IStatisticsService _service;

    public UsageVariableProviderTests()
    {
        _repository = A.Fake<IRepository>();
        _repositoryContext = new RepositoryContext(_repository);
        _service = A.Fake<IStatisticsService>();
        _sut = new UsageVariableProvider(_service);
    }

    [Theory]
    [InlineData("usage")]
    [InlineData("statistics.count")]
    [InlineData("statistics.totalcount")]
    public void CanProvide_ShouldReturnTrue_WhenKeyIsValid(string key)
    {
        // arrange

        // act
        var result = _sut.CanProvide(key);

        // assert
        result.Should().BeTrue();
    }

    [Theory]
    [InlineData("usagex")]
    [InlineData("statistics.countx")]
    [InlineData("statisticscount")]
    [InlineData("statistics.totalcountx")]
    [InlineData("statisticstotalcount")]
    [InlineData("")]
    [InlineData("  ")]
    public void CanProvide_ShouldReturnFalse_WhenKeyIsInValid(string key)
    {
        // arrange
        
        // act
        var result = _sut.CanProvide(key);

        // assert
        result.Should().BeFalse();
    }
    
    [Fact]
    public void Provide_ShouldThrowNotImplementedException()
    {
        // arrange

        // act
        Action act = () => _sut.Provide("any", null);

        // assert
        act.Should().Throw<NotImplementedException>();
    }

    [Theory]
    [InlineData("usage")] // obsolete
    [InlineData("statistics.count")]
    [InlineData("statistics.Count")]
    public void Provide_ShouldReturnCount_WhenKeyHasValue(string key)
    {
        // arrange
        A.CallTo(() => _service.GetRecordings(_repository))
         .Returns(new List<DateTime> { DateTime.Now, DateTime.Now, });

        // act
        var result = _sut.Provide(_repositoryContext, key, null);

        // assert
        result.Should().BeOfType<int>().And.Be(2);
        A.CallTo(() => _service.GetRecordings(_repository)).MustHaveHappenedOnceExactly();
        A.CallTo(_service).MustHaveHappenedOnceExactly();
    }

    [Theory]
    [InlineData("statistics.totalcount")]
    [InlineData("statistics.totalCount")]
    public void Provide_ShouldReturnTotalCount_WhenKeyHasValue(string key)
    {
        // arrange
        A.CallTo(() => _service.GetTotalRecordingCount()).Returns(42);

        // act
        var result = _sut.Provide(_repositoryContext, key, null);

        // assert
        result.Should().BeOfType<int>().And.Be(42);
        A.CallTo(() => _service.GetTotalRecordingCount()).MustHaveHappenedOnceExactly();
        A.CallTo(_service).MustHaveHappenedOnceExactly();
    }
}