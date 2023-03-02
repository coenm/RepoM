namespace RepoM.Plugin.AzureDevOps.Tests.ActionProvider;

using System;
using System.Collections.Generic;
using FakeItEasy;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using RepoM.Api.Git;
using RepoM.Api.IO.ModuleBasedRepositoryActionProvider;
using RepoM.Api.IO.ModuleBasedRepositoryActionProvider.ActionMappers;
using RepoM.Core.Plugin.Expressions;
using RepoM.Core.Plugin.Repository;
using RepoM.Plugin.AzureDevOps.ActionProvider;
using RepoM.Plugin.AzureDevOps.Internal;
using Xunit;
using RepositoryAction = RepoM.Api.IO.ModuleBasedRepositoryActionProvider.Data.RepositoryAction;

public class ActionAzureDevOpsPullRequestsV1MapperTests
{
    private readonly IAzureDevOpsPullRequestService _service;
    private readonly IRepositoryExpressionEvaluator _evaluator;
    private readonly ActionAzureDevOpsPullRequestsV1Mapper _sut;
    private readonly RepositoryActionAzureDevOpsPullRequestsV1 _action;
    private readonly IEnumerable<Repository> _repositories;
    private readonly Repository _repository;
    private readonly ActionMapperComposition _composition;

    public ActionAzureDevOpsPullRequestsV1MapperTests()
    {
        _service = A.Fake<IAzureDevOpsPullRequestService>();
        _evaluator = A.Fake<IRepositoryExpressionEvaluator>();
        _sut = new ActionAzureDevOpsPullRequestsV1Mapper(_service, _evaluator, NullLogger.Instance);

        _action = new RepositoryActionAzureDevOpsPullRequestsV1();
        _repository = new Repository("");
        _repositories = new [] { _repository, };
        _composition = new ActionMapperComposition(Array.Empty<IActionToRepositoryActionMapper>(), _evaluator);

        // default test behavior.
        _action.Active = "dummy-Active-property";
        _action.ProjectId = "dummy-project-id";
        A.CallTo(() => _evaluator.EvaluateBooleanExpression("dummy-Active-property", _repository)).Returns(true);
        A.CallTo(() => _evaluator.EvaluateStringExpression("dummy-project-id", A<IRepository[]>._)).Returns("real-project-id");
    }

    [Fact]
    public void CanHandleMultipleRepositories_ShouldReturnFalse()
    {
        // arrange

        // act
        var result = _sut.CanHandleMultipleRepositories();

        // assert
        result.Should().BeFalse();
        A.CallTo(_service).MustNotHaveHappened();
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
        A.CallTo(_service).MustNotHaveHappened();
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
        A.CallTo(_service).MustNotHaveHappened();
    }

    [Fact]
    public void Map_ShouldReturnEmptySet_WhenWrongActionType()
    {
        // arrange

        // act
        IEnumerable<RepositoryActionBase> result = _sut.Map(new DummyRepositoryAction(), _repositories, _composition);

        // assert
        result.Should().BeEmpty();
        A.CallTo(_service).MustNotHaveHappened();
    }

    [Fact]
    public void Map_ShouldReturnEmptySet_WhenActionNotActive()
    {
        // arrange
        _action.Active = "dummy";
        A.CallTo(() => _evaluator.EvaluateBooleanExpression("dummy", _repository)).Returns(false);

        // act
        IEnumerable<RepositoryActionBase> result = _sut.Map(_action, _repositories, _composition);

        // assert
        result.Should().BeEmpty();
        A.CallTo(_service).MustNotHaveHappened();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Map_ShouldReturnEmptySet_WhenProjectIdNotSet(string? projectId)
    {
        // arrange
        _action.ProjectId = projectId;

        // act
        IEnumerable<RepositoryActionBase> result = _sut.Map(_action, _repositories, _composition);

        // assert
        result.Should().BeEmpty();
        A.CallTo(_service).MustNotHaveHappened();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Map_ShouldReturnEmptySet_WhenProjectIdIsNotValidAfterEvaluation(string? projectId)
    {
        // arrange
        _action.ProjectId = "dummy-project-id";
        A.CallTo(() => _evaluator.EvaluateStringExpression("dummy-project-id", A<IRepository[]>._)).Returns(projectId!);

        // act
        IEnumerable<RepositoryActionBase> result = _sut.Map(_action, _repositories, _composition);

        // assert
        result.Should().BeEmpty();
        A.CallTo(_service).MustNotHaveHappened();
    }
}

file class DummyRepositoryAction : RepositoryAction
{
}