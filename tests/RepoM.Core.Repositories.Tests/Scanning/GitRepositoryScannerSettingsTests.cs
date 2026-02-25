namespace RepoM.Core.Repositories.Tests.Scanning;

using System;
using AwesomeAssertions;
using RepoM.Core.Repositories.Scanning;
using Xunit;

public class GitRepositoryScannerSettingsTests
{
    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(8)]
    [InlineData(64)]
    public void Ctor_ShouldSetDegreeOfParallelism(int degreeOfParallelism)
    {
        // act
        var sut = new GitRepositoryScannerSettings(degreeOfParallelism);

        // assert
        sut.DegreeOfParallelism.Should().Be(degreeOfParallelism);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void Ctor_ShouldThrow_WhenDegreeOfParallelismIsLessThanOne(int degreeOfParallelism)
    {
        // act
        Func<GitRepositoryScannerSettings> act = () => new GitRepositoryScannerSettings(degreeOfParallelism);

        // assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }
}