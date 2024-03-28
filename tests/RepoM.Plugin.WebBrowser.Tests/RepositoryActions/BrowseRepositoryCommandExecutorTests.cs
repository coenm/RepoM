namespace RepoM.Plugin.WebBrowser.Tests.RepositoryActions;

using System;
using FakeItEasy;
using FluentAssertions;
using RepoM.Core.Plugin.Repository;
using RepoM.Plugin.WebBrowser.RepositoryActions;
using RepoM.Plugin.WebBrowser.RepositoryActions.Actions;
using RepoM.Plugin.WebBrowser.Services;
using Xunit;

public class BrowseRepositoryCommandExecutorTests
{
    private readonly IRepository _repository;
    private readonly IWebBrowserService _service;
    private readonly BrowseRepositoryCommandExecutor _sut;

    public BrowseRepositoryCommandExecutorTests()
    {
        _repository = A.Fake<IRepository>();
        _service = A.Fake<IWebBrowserService>();
        _sut = new BrowseRepositoryCommandExecutor(_service);
    }

    [Fact]
    public void Ctor_ShouldThrow_WhenArgumentNull()
    {
        // arrange

        // act
        Func<BrowseRepositoryCommandExecutor> act1 = () => new BrowseRepositoryCommandExecutor(null!);

        // assert
        act1.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Execute_ShouldCallOpenUrlWithoutProfile_WhenProfileIsNull()
    {
        // arrange

        // act
        _sut.Execute(_repository, new BrowseRepositoryCommand("url", null));

        // assert
        A.CallTo(() => _service.OpenUrl("url")).MustHaveHappenedOnceExactly();
        A.CallTo(() => _service.OpenUrl(A<string>._, A<string>._)).MustNotHaveHappened();
    }

    [Theory]
    [InlineData("profile")]
    [InlineData("")]
    [InlineData(" ")]
    public void Execute_ShouldCallOpenUrlWithProfile_WhenProfileIsNotNull(string profile)
    {
        // arrange

        // act
        _sut.Execute(_repository, new BrowseRepositoryCommand("url", profile));

        // assert
        A.CallTo(() => _service.OpenUrl(A<string>._)).MustNotHaveHappened();
        A.CallTo(() => _service.OpenUrl("url", profile)).MustHaveHappenedOnceExactly();
    }
}