namespace RepoM.Plugin.EverythingFileSearch.Tests.Internal;

using System;
using FakeItEasy;
using FluentAssertions;
using RepoM.Core.Plugin.RepositoryFinder;
using RepoM.Plugin.EverythingFileSearch.Internal;
using Xunit;

public class EverythingGitRepositoryFinderTests
{
    [Fact]
    public void Ctor_ShouldThrow_WhenArgumentIsNull()
    {
        // arrange

        // act
        Action act = () => _ = new EverythingGitRepositoryFinder(null!);

        // assert
        act.Should().ThrowExactly<ArgumentNullException>();
    }

    [Fact]
    public void Ctor_ShouldNotThrown_WhenArgumentIsNotNull()
    {
        // arrange

        // act
        Action act = () => _ = new EverythingGitRepositoryFinder(A.Dummy<IPathSkipper>());

        // assert
        act.Should().NotThrow();
    }
}