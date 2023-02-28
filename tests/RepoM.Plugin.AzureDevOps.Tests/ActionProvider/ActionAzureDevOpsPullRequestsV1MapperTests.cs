namespace RepoM.Plugin.AzureDevOps.Tests.ActionProvider;

using FakeItEasy;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using RepoM.Api.Common;
using RepoM.Api.IO.ModuleBasedRepositoryActionProvider.Data;
using RepoM.Core.Plugin.Expressions;
using RepoM.Plugin.AzureDevOps.ActionProvider;
using RepoM.Plugin.AzureDevOps.Internal;
using Xunit;

public class ActionAzureDevOpsPullRequestsV1MapperTests
{
    private readonly IAzureDevOpsPullRequestService _service;
    private readonly IRepositoryExpressionEvaluator _evaluator;
    private readonly ITranslationService _translation;
    private readonly ActionAzureDevOpsPullRequestsV1Mapper _sut;

    public ActionAzureDevOpsPullRequestsV1MapperTests()
    {
        _service = A.Fake<IAzureDevOpsPullRequestService>();
        _evaluator = A.Fake<IRepositoryExpressionEvaluator>();
        _translation = A.Fake<ITranslationService>();
        _sut = new ActionAzureDevOpsPullRequestsV1Mapper(_service, _evaluator, _translation, NullLogger.Instance);
    }

    [Fact]
    public void CanHandleMultipleRepositories_ShouldReturnFalse()
    {
        // arrange

        // act
        var result = _sut.CanHandleMultipleRepositories();

        // assert
        result.Should().BeFalse();
    }

    [Fact]
    public void CanMap_ShouldReturnFalse_WhenInputIsNotValidType()
    {
        // arrange
        var action = new DummyRepositoryAction();

        // act
        var result = _sut.CanMap(action);

        // assert
        result.Should().BeFalse();
    }

    [Fact]
    public void CanMap_ShouldReturnTrue_WhenInputIsValidType()
    {
        // arrange
        var action = new RepositoryActionAzureDevOpsPullRequestsV1();

        // act
        var result = _sut.CanMap(action);

        // assert
        result.Should().BeTrue();
    }
}

file class DummyRepositoryAction : RepositoryAction
{
}