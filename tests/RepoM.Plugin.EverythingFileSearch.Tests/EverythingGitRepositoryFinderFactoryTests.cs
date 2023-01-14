namespace RepoM.Plugin.EverythingFileSearch.Tests;

using System;
using System.Net.NetworkInformation;
using FakeItEasy;
using FluentAssertions;
using RepoM.Core.Plugin.RepositoryFinder;
using RepoM.Plugin.EverythingFileSearch.Internal;
using Xunit;

public class EverythingGitRepositoryFinderFactoryTests
{
    private readonly IPathSkipper _pathSkipper;

    public EverythingGitRepositoryFinderFactoryTests()
    {
        _pathSkipper = A.Fake<IPathSkipper>();
    }

    [Fact]
    public void Ctor_ShouldThrow_WhenArgumentIsNull()
    {
        // arrange

        // act
        Action act = () => _ = new EverythingGitRepositoryFinderFactory(null!);

        // assert
        act.Should().ThrowExactly<ArgumentNullException>();
    }

    [Fact]
    public void Name_ShouldBeEverything()
    {
        // arrange
        var sut = new EverythingGitRepositoryFinderFactory(_pathSkipper);

        // act
        var result = sut.Name;

        // assert
        result.Should().Be("Everything");
    }

    [Fact]
    public void IsActive_ShouldReturnSameValueAsEverythingApi()
    {
        // arrange
        var sut = new EverythingGitRepositoryFinderFactory(_pathSkipper);

        // act
        var result = sut.IsActive;

        // assert
        result.Should().Be(Everything64Api.IsInstalled());
    }

    [Fact]
    public void Create_ShouldReturnSameValueAsEverythingApi()
    {
        // arrange
        var sut = new EverythingGitRepositoryFinderFactory(_pathSkipper);

        // act
        IGitRepositoryFinder result = sut.Create();

        // assert
        result.Should().NotBeNull().And.BeOfType<EverythingGitRepositoryFinder>();
    }
}