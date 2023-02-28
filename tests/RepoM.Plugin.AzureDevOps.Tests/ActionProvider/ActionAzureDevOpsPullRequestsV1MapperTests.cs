namespace RepoM.Plugin.AzureDevOps.Tests.ActionProvider;

using System;
using System.Collections.Generic;
using FakeItEasy;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using RepoM.Api.Common;
using RepoM.Api.Git;
using RepoM.Api.IO.ModuleBasedRepositoryActionProvider;
using RepoM.Api.IO.ModuleBasedRepositoryActionProvider.ActionMappers;
using RepoM.Core.Plugin.Expressions;
using RepoM.Plugin.AzureDevOps.ActionProvider;
using RepoM.Plugin.AzureDevOps.Internal;
using Xunit;
using RepositoryAction = RepoM.Api.IO.ModuleBasedRepositoryActionProvider.Data.RepositoryAction;

public class ActionAzureDevOpsPullRequestsV1MapperTests
{
    private readonly IAzureDevOpsPullRequestService _service;
    private readonly IRepositoryExpressionEvaluator _evaluator;
    private readonly ITranslationService _translation;
    private readonly ActionAzureDevOpsPullRequestsV1Mapper _sut;
    private readonly RepositoryAction _action;
    private readonly IEnumerable<Repository> _repositories;
    private readonly Repository _repository;
    private readonly ActionMapperComposition _composition;

    public ActionAzureDevOpsPullRequestsV1MapperTests()
    {
        _service = A.Fake<IAzureDevOpsPullRequestService>();
        _evaluator = A.Fake<IRepositoryExpressionEvaluator>();
        _translation = A.Fake<ITranslationService>();
        _sut = new ActionAzureDevOpsPullRequestsV1Mapper(_service, _evaluator, _translation, NullLogger.Instance);

        _action = new RepositoryActionAzureDevOpsPullRequestsV1();
        _repository = new Repository("");
        _repositories = new [] { _repository, };
        _composition = new ActionMapperComposition(Array.Empty<IActionToRepositoryActionMapper>(), _evaluator);
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

    [Fact]
    public void Map_Should_When_Todo()
    {
        // arrange

        // act
        IEnumerable<RepositoryActionBase> result = _sut.Map(_action, _repositories, _composition);

        // assert
        result.Should().BeEmpty();
    }
}

file class DummyRepositoryAction : RepositoryAction
{
}