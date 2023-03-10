namespace RepoM.Plugin.Statistics.Tests.VariableProviders;

using FakeItEasy;
using FluentAssertions;
using RepoM.Plugin.Statistics.VariableProviders;
using Xunit;

public class UsageVariableProviderTests
{
    [Theory]
    [InlineData("usage")]
    [InlineData("statistics.count")]
    [InlineData("statistics.totalcount")]
    public void CanProvide_ShouldReturnTrue_WhenKeyIsValid(string key)
    {
        // arrange
        var sut = new UsageVariableProvider(A.Dummy<IStatisticsService>());

        // act
        var result = sut.CanProvide(key);

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
        var sut = new UsageVariableProvider(A.Dummy<IStatisticsService>());

        // act
        var result = sut.CanProvide(key);

        // assert
        result.Should().BeFalse();
    }
}