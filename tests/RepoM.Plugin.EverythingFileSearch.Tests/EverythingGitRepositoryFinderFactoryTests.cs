namespace RepoM.Plugin.EverythingFileSearch.Tests;

using System;
using FluentAssertions;
using Xunit;

public class EverythingGitRepositoryFinderFactoryTests
{
    [Fact]
    public void Ctor_ShouldThrow_WhenArgumentIsNull()
    {
        // arrange

        // act
        Action act = () => _ = new EverythingGitRepositoryFinderFactory(null!);

        // assert
        act.Should().ThrowExactly<ArgumentNullException>();
    }
}