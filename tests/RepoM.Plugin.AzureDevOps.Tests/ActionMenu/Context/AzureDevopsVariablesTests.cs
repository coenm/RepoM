namespace RepoM.Plugin.AzureDevOps.Tests.ActionMenu.Context;

using System;
using System.Collections;
using System.Collections.Generic;
using FakeItEasy;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using RepoM.ActionMenu.Interface.ActionMenuFactory;
using RepoM.Core.Plugin.Repository;
using RepoM.Plugin.AzureDevOps.ActionMenu.Context;
using RepoM.Plugin.AzureDevOps.Internal;
using Xunit;

public class AzureDevopsVariablesTests
{
    private readonly ILogger _logger = NullLogger.Instance;
    private readonly IAzureDevOpsPullRequestService _service;
    private readonly IActionMenuGenerationContext _context;
    private readonly AzureDevopsVariables _sut;

    public AzureDevopsVariablesTests()
    {
        _context = A.Fake<IActionMenuGenerationContext>();
        A.CallTo(() => _context.Repository).Returns(A.Fake<IRepository>());
        _service = A.Fake<IAzureDevOpsPullRequestService>();
        _sut = new AzureDevopsVariables(_service, _logger);
    }

    [Fact]
    public void Ctor_ShouldThrow_WhenArgumentNull()
    {
        // arrange

        // act
        Func<AzureDevopsVariables> act1 = () => new AzureDevopsVariables(A.Dummy<IAzureDevOpsPullRequestService>(), null!);
        Func<AzureDevopsVariables> act2 = () => new AzureDevopsVariables(null!, A.Dummy<ILogger>());

        // assert
        act1.Should().Throw<ArgumentNullException>();
        act2.Should().Throw<ArgumentNullException>();
    }
    
    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void GetPullRequests_ShouldReturnEmpty_WhenProjectIdIsInvalid(string? projectId)
    {
        // arrange

        // act
        IEnumerable result = _sut.GetPullRequests(_context, projectId!);

        // assert
        result.Should().BeEquivalentTo(Array.Empty<object>());
        A.CallTo(_service).MustNotHaveHappened();
    }
    
    [Fact]
    public void GetPullRequests_ShouldCallAndReturnServiceGetPullRequests()
    {
        // arrange
        var prs = new List<PullRequest>()
            {
                new PullRequest(Guid.Empty, "some pr1", "https://google.com/pr1"),
            };
        A.CallTo(() => _service.GetPullRequests(_context.Repository, "my_project_id", null)).Returns(prs);

        // act
        IEnumerable result = _sut.GetPullRequests(_context, "my_project_id");

        // assert
        result.Should().BeEquivalentTo(prs);
    }

    [Fact]
    public void GetPullRequests_ShouldReturnEmpty_WhenServiceReturnsNull()
    {
        // arrange
        A.CallTo(() => _service.GetPullRequests(_context.Repository, "my_project_id", null)).Returns(null);

        // act
        IEnumerable result = _sut.GetPullRequests(_context, "my_project_id");

        // assert
        result.Should().BeEquivalentTo(Array.Empty<object>());
    }

    [Fact]
    public void GetPullRequests_ShouldReturnEmpty_WhenServiceThrows()
    {
        // arrange
        A.CallTo(() => _service.GetPullRequests(_context.Repository, "my_project_id", null)).Throws(new Exception("thrown by test"));

        // act
        IEnumerable result = _sut.GetPullRequests(_context, "my_project_id");

        // assert
        result.Should().BeEquivalentTo(Array.Empty<object>());
    }
}