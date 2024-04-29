namespace RepoM.Api.Tests.IO;

using System;
using FakeItEasy;
using FluentAssertions;
using RepoM.Api.IO;
using RepoM.Core.Plugin.RepositoryFinder;
using Xunit;

public class GitRepositoryFinderFactoryTests
{
    private readonly ISingleGitRepositoryFinderFactory _singleGitRepositoryFinderFactory;

    public GitRepositoryFinderFactoryTests()
    {
        _singleGitRepositoryFinderFactory = A.Fake<ISingleGitRepositoryFinderFactory>();
    }

    [Fact]
    public void Ctor_ShouldThrow_WhenArgumentNull()
    {
        // arrange

        // act
        Func<GitRepositoryFinderFactory> act1 = () => new GitRepositoryFinderFactory(null!);

        // assert
        act1.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Create_ShouldReturnCreationOfFirstFactory_WhenFirstFactoryIsEnabled()
    {
        // arrange
        IGitRepositoryFinder gitRepositoryFinder = A.Fake<IGitRepositoryFinder>();
        A.CallTo(() => _singleGitRepositoryFinderFactory.Create()).Returns(gitRepositoryFinder);
        var sut = new GitRepositoryFinderFactory(_singleGitRepositoryFinderFactory);

        // act
        IGitRepositoryFinder result = sut.Create();

        // assert
        result.Should().BeSameAs(gitRepositoryFinder);
    }
}