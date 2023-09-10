namespace RepoM.Api.Tests.IO.ModuleBasedRepositoryActionProvider.ActionMappers;

using System;
using FakeItEasy;
using FluentAssertions;
using RepoM.Api.Common;
using RepoM.Api.Git;
using RepoM.Api.IO.ModuleBasedRepositoryActionProvider;
using RepoM.Api.IO.ModuleBasedRepositoryActionProvider.ActionMappers;
using RepoM.Api.IO.ModuleBasedRepositoryActionProvider.Data.Actions;
using RepoM.Core.Plugin.Expressions;
using Xunit;

public class ActionGitCheckoutV1MapperTests
{
    private readonly IRepositoryExpressionEvaluator _expressionEvaluator;
    private readonly ITranslationService _translationService;
    private readonly IRepositoryWriter _repositoryWriter;

    public ActionGitCheckoutV1MapperTests()
    {
        _expressionEvaluator = A.Fake<IRepositoryExpressionEvaluator>();
        _translationService = A.Fake<ITranslationService>();
        _repositoryWriter = A.Fake<IRepositoryWriter>();
    }

    [Fact]
    public void Ctor_ShouldThrown_WhenArgumentIsNull()
    {
        // arrange

        // act
        Action act1 = () => _ = new ActionGitCheckoutV1Mapper(_expressionEvaluator, _translationService, null!);
        Action act2 = () => _ = new ActionGitCheckoutV1Mapper(_expressionEvaluator, null!, _repositoryWriter);
        Action act3 = () => _ = new ActionGitCheckoutV1Mapper(null!, _translationService, _repositoryWriter);

        // assert
        act1.Should().ThrowExactly<ArgumentNullException>();
        act2.Should().ThrowExactly<ArgumentNullException>();
        act3.Should().ThrowExactly<ArgumentNullException>();
    }
    
    [Fact]
    public void CanMap_ShouldReturnFalse_WhenInputNull()
    {
        // arrange
        var sut = new ActionGitCheckoutV1Mapper(_expressionEvaluator, _translationService, _repositoryWriter) as IActionToRepositoryActionMapper;

        // act
        var result = sut.CanMap(null!);

        // assert
        result.Should().BeFalse();
    }
    
    [Fact]
    public void CanMap_ShouldReturnFalse_WhenInputIsNotCorrectType()
    {
        // arrange

        var sut = new ActionGitCheckoutV1Mapper(_expressionEvaluator, _translationService, _repositoryWriter) as IActionToRepositoryActionMapper;

        // act
        var result = sut.CanMap(new DummyRepositoryAction());

        // assert
        result.Should().BeFalse();
    }
    
    [Fact]
    public void CanMap_ShouldReturnTrue_WhenInputIsOfTypeRepositoryActionGitCheckoutV1()
    {
        // arrange
        var sut = new ActionGitCheckoutV1Mapper(_expressionEvaluator, _translationService, _repositoryWriter) as IActionToRepositoryActionMapper;

        // act
        var result = sut.CanMap(new RepositoryActionGitCheckoutV1());

        // assert
        result.Should().BeTrue();
    }
}

file class DummyRepositoryAction : Api.IO.ModuleBasedRepositoryActionProvider.Data.RepositoryAction
{
}