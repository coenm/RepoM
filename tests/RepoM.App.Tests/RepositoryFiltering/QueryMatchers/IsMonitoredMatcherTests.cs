namespace RepoM.App.Tests.RepositoryFiltering.QueryMatchers;

using System;
using FakeItEasy;
using AwesomeAssertions;
using RepoM.App.RepositoryFiltering.QueryMatchers;
using RepoM.Core.Plugin.Repository;
using RepoM.Core.Plugin.RepositoryFiltering.Clause.Terms;
using RepoM.Core.Repositories.Monitoring;
using Xunit;

public class IsMonitoredMatcherTests
{
    private readonly IRepository _repository = A.Fake<IRepository>();
    private readonly IRepositoryMonitoringService _monitoringService = A.Fake<IRepositoryMonitoringService>();
    private readonly IsMonitoredMatcher _sut;

    public IsMonitoredMatcherTests()
    {
        _sut = new IsMonitoredMatcher(_monitoringService);
    }

    [Fact]
    public void Ctor_ShouldThrow_WhenArgumentNull()
    {
        // arrange

        // act
        Func<IsMonitoredMatcher> act = () => new IsMonitoredMatcher(null!);

        // assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void IsMatch_ShouldReturnNull_WhenTermIsNotSimpleTerm()
    {
        // arrange
        TermBase term = A.Fake<TermBase>();

        // act
        var result = _sut.IsMatch(in _repository, in term);

        // assert
        result.Should().BeNull();
        A.CallTo(_monitoringService).MustNotHaveHappened();
    }

    [Theory]
    [InlineData("", "")]
    [InlineData("bla", "")]
    [InlineData("is", "")]
    [InlineData("", "bla")]
    [InlineData("x", "active")]
    [InlineData("", "active")]
    [InlineData("x", "inactive")]
    [InlineData("", "inactive")]
    [InlineData("is", "aactive")]
    [InlineData("is", "Active")] // invalid casing
    [InlineData("Is", "active")] // invalid casing
    [InlineData("is", "Inactive")] // invalid casing
    [InlineData("Is", "inactive")] // invalid casing
    public void IsMatch_ShouldReturnNull_WhenTermAndValueDoNotMatch(string term, string value)
    {
        // arrange
        TermBase simpleTerm = new SimpleTerm(term, value);

        // act
        var result = _sut.IsMatch(in _repository, in simpleTerm);

        // assert
        result.Should().BeNull();
        A.CallTo(_monitoringService).MustNotHaveHappened();
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void IsMatch_ShouldReturnIsMonitoredValue_WhenTermIsActive(bool isMonitored)
    {
        // arrange
        A.CallTo(() => _repository.SafePath).Returns("/safe/path");
        A.CallTo(() => _monitoringService.IsMonitored("/safe/path")).Returns(isMonitored);
        TermBase simpleTerm = new SimpleTerm("is", "active");

        // act
        var result = _sut.IsMatch(in _repository, in simpleTerm);

        // assert
        result.Should().Be(isMonitored);
        A.CallTo(() => _monitoringService.IsMonitored("/safe/path")).MustHaveHappenedOnceExactly();
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void IsMatch_ShouldReturnNegatedIsMonitoredValue_WhenTermIsInactive(bool isMonitored)
    {
        // arrange
        A.CallTo(() => _repository.SafePath).Returns("/safe/path");
        A.CallTo(() => _monitoringService.IsMonitored("/safe/path")).Returns(isMonitored);
        TermBase simpleTerm = new SimpleTerm("is", "inactive");

        // act
        var result = _sut.IsMatch(in _repository, in simpleTerm);

        // assert
        result.Should().Be(!isMonitored);
        A.CallTo(() => _monitoringService.IsMonitored("/safe/path")).MustHaveHappenedOnceExactly();
    }
}
