namespace RepoM.Plugin.WebBrowser.Tests.RepositoryActions;

using System;
using FakeItEasy;
using FluentAssertions;
using RepoM.Core.Plugin.Repository;
using RepoM.Plugin.WebBrowser.RepositoryActions;
using RepoM.Plugin.WebBrowser.RepositoryActions.Actions;
using RepoM.Plugin.WebBrowser.Services;
using Xunit;

public class BrowseActionExecutorTests
{
    private readonly IRepository _repository;
    private readonly IWebBrowserService _service;
    private readonly BrowseActionExecutor _sut;

    public BrowseActionExecutorTests()
    {
        _repository = A.Fake<IRepository>();
        _service = A.Fake<IWebBrowserService>();
        _sut = new BrowseActionExecutor(_service);
    }

    [Fact]
    public void Ctor_ShouldThrow_WhenArgumentNull()
    {
        // arrange

        // act
        Func<BrowseActionExecutor> act1 = () => new BrowseActionExecutor(null!);

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