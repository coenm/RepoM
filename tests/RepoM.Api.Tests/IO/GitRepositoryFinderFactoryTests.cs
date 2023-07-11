namespace RepoM.Api.Tests.IO;

using System;
using System.Collections.Generic;
using FakeItEasy;
using FluentAssertions;
using RepoM.Api.Common;
using RepoM.Api.IO;
using RepoM.Core.Plugin.RepositoryFiltering.Clause.Terms;
using RepoM.Core.Plugin.RepositoryFinder;
using Xunit;

public class GitRepositoryFinderFactoryTests
{
    private readonly IAppSettingsService _appSettingsService;
    private readonly ISingleGitRepositoryFinderFactory _singleGitRepositoryFinderFactory1;
    private readonly ISingleGitRepositoryFinderFactory _singleGitRepositoryFinderFactory2;

    public GitRepositoryFinderFactoryTests()
    {
        _appSettingsService = A.Fake<IAppSettingsService>();
        _singleGitRepositoryFinderFactory1 = A.Fake<ISingleGitRepositoryFinderFactory>();
        _singleGitRepositoryFinderFactory2 = A.Fake<ISingleGitRepositoryFinderFactory>();
    }

    [Fact]
    public void Ctor_ShouldThrow_WhenArgumentNull()
    {
        // arrange

        // act
        Func<GitRepositoryFinderFactory> act1 = () => new GitRepositoryFinderFactory(A.Dummy<IAppSettingsService>(), null!);
        Func<GitRepositoryFinderFactory> act2 = () => new GitRepositoryFinderFactory(null!, new[] { A.Dummy<ISingleGitRepositoryFinderFactory>(), });

        // assert
        act1.Should().Throw<ArgumentNullException>();
        act2.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Create_ShouldReturnCreationOfFirstFactory_WhenFirstFactoryIsEnabled()
    {
        // arrange
        IGitRepositoryFinder gitRepositoryFinder = A.Fake<IGitRepositoryFinder>();
        A.CallTo(() => _appSettingsService.EnabledSearchProviders).Returns(new List<string>() { "Dummy123", "Dummy555", });
        A.CallTo(() => _singleGitRepositoryFinderFactory1.IsActive).Returns(true);
        A.CallTo(() => _singleGitRepositoryFinderFactory1.Name).Returns("Dummy123");
        A.CallTo(() => _singleGitRepositoryFinderFactory1.Create()).Returns(gitRepositoryFinder);
        var sut = new GitRepositoryFinderFactory(_appSettingsService, new[] { _singleGitRepositoryFinderFactory1, _singleGitRepositoryFinderFactory2, });

        // act
        IGitRepositoryFinder result = sut.Create();

        // assert
        result.Should().BeSameAs(gitRepositoryFinder);
    }
}