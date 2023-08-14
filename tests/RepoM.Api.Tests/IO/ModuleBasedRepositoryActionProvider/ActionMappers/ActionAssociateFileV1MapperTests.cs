namespace RepoM.Api.Tests.IO.ModuleBasedRepositoryActionProvider.ActionMappers;

using System;
using System.Linq;
using FakeItEasy;
using FluentAssertions;
using RepoM.Api.Common;
using RepoM.Api.Git;
using RepoM.Api.IO.ModuleBasedRepositoryActionProvider;
using RepoM.Api.IO.ModuleBasedRepositoryActionProvider.ActionMappers;
using RepoM.Api.IO.ModuleBasedRepositoryActionProvider.Data.Actions;
using RepoM.Core.Plugin.Expressions;
using Xunit;
using Repository = RepoM.Api.Git.Repository;

public class ActionAssociateFileV1MapperTests
{
    private readonly IRepositoryExpressionEvaluator _expressionEvaluator;
    private readonly ITranslationService _translationService;
    private Repository _repository;

    public ActionAssociateFileV1MapperTests()
    {
        _repository = new Repository("");
        _expressionEvaluator = A.Fake<IRepositoryExpressionEvaluator>();
        _translationService = A.Fake<ITranslationService>();
        A.CallTo(() => _translationService.Translate(A<string>._, A<object[]>._)).ReturnsLazily(call => call.Arguments[0] as string ?? "dummy");
        A.CallTo(() => _translationService.Translate(A<string>._)).ReturnsLazily(call => call.Arguments[0] as string ?? "dummy");
    }

    [Fact]
    public void Ctor_ShouldThrown_WhenArgumentIsNull()
    {
        // arrange

        // act
        Action act1 = () => _ = new ActionAssociateFileV1Mapper(_expressionEvaluator, null!);
        Action act2 = () => _ = new ActionAssociateFileV1Mapper(null!, _translationService);

        // assert
        act1.Should().ThrowExactly<ArgumentNullException>();
        act2.Should().ThrowExactly<ArgumentNullException>();
    }
    
    [Fact]
    public void CanHandleMultipleRepositories_ShouldReturnFalse()
    {
        // arrange
        var sut = new ActionAssociateFileV1Mapper(_expressionEvaluator, _translationService) as IActionToRepositoryActionMapper;

        // act
        var result = sut.CanHandleMultipleRepositories();

        // assert
        result.Should().BeFalse();
    }
    
    [Fact]
    public void CanMap_ShouldReturnFalse_WhenInputNull()
    {
        // arrange
        var sut = new ActionAssociateFileV1Mapper(_expressionEvaluator, _translationService) as IActionToRepositoryActionMapper;

        // act
        var result = sut.CanMap(null!);

        // assert
        result.Should().BeFalse();
    }
    
    [Fact]
    public void CanMap_ShouldReturnFalse_WhenInputIsNotCorrectType()
    {
        // arrange

        var sut = new ActionAssociateFileV1Mapper(_expressionEvaluator, _translationService) as IActionToRepositoryActionMapper;

        // act
        var result = sut.CanMap(new DummyRepositoryAction());

        // assert
        result.Should().BeFalse();
    }

    [Fact]
    public void CanMap_ShouldReturnTrue_WhenInputIsOfTypeRepositoryActionAssociateFileV1()
    {
        // arrange
        var sut = new ActionAssociateFileV1Mapper(_expressionEvaluator, _translationService) as IActionToRepositoryActionMapper;

        // act
        var result = sut.CanMap(new RepositoryActionAssociateFileV1());

        // assert
        result.Should().BeTrue();
    }

    [Fact]
    public void Map_ShouldReturnEmpty_WhenActionIsNotOfCorrectType()
    {
        // arrange
        var action = new DummyRepositoryAction();
        var sut = new ActionAssociateFileV1Mapper(_expressionEvaluator, _translationService) as IActionToRepositoryActionMapper;

        // act
        RepositoryActionBase[] result = sut.Map(action, new [] { _repository, }, ActionMapperCompositionFactory.CreateSmall(_expressionEvaluator, A.Dummy<IActionToRepositoryActionMapper>())).ToArray();

        // assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void Map_ShouldReturnEmpty_WhenActionActiveIsEvaluatesToFalse()
    {
        // arrange
        var action = new RepositoryActionAssociateFileV1()
            {
                Active = "dummy active string",
            };
        A.CallTo(() => _expressionEvaluator.EvaluateBooleanExpression(action.Active, _repository)).Returns(false);
        var sut = new ActionAssociateFileV1Mapper(_expressionEvaluator, _translationService) as IActionToRepositoryActionMapper;

        // act
        RepositoryActionBase[] result = sut.Map(action, new [] { _repository, }, ActionMapperCompositionFactory.CreateSmall(_expressionEvaluator, A.Dummy<IActionToRepositoryActionMapper>())).ToArray();

        // assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void Map_ShouldReturnEmpty_WhenActionExtensionIsNull()
    {
        // arrange
        var action = new RepositoryActionAssociateFileV1()
            {
                Extension = null,
            };
        A.CallTo(() => _expressionEvaluator.EvaluateBooleanExpression(A<string?>._, _repository)).Returns(true);
        var sut = new ActionAssociateFileV1Mapper(_expressionEvaluator, _translationService) as IActionToRepositoryActionMapper;

        // act
        RepositoryActionBase[] result = sut.Map(action, new [] { _repository, }, ActionMapperCompositionFactory.CreateSmall(_expressionEvaluator, A.Dummy<IActionToRepositoryActionMapper>())).ToArray();

        // assert
        result.Should().BeEmpty();
    }
}

file class DummyRepositoryAction : Api.IO.ModuleBasedRepositoryActionProvider.Data.RepositoryAction
{
}