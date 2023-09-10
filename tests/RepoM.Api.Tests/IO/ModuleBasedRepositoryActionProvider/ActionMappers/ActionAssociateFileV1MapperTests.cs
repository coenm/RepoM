namespace RepoM.Api.Tests.IO.ModuleBasedRepositoryActionProvider.ActionMappers;

using System;
using System.Linq;
using FakeItEasy;
using FluentAssertions;
using RepoM.Api.Git;
using RepoM.Api.IO.ModuleBasedRepositoryActionProvider;
using RepoM.Api.IO.ModuleBasedRepositoryActionProvider.ActionMappers;
using RepoM.Api.IO.ModuleBasedRepositoryActionProvider.Data.Actions;
using RepoM.Core.Plugin.Expressions;
using Xunit;
using Repository = RepoM.Api.Git.Repository;

public class ActionAssociateFileV1MapperTests
{
    private readonly IRepositoryExpressionEvaluator _expressionEvaluator = A.Fake<IRepositoryExpressionEvaluator>();
    private readonly Repository _repository = new("");

    [Fact]
    public void Ctor_ShouldThrown_WhenArgumentIsNull()
    {
        // arrange

        // act
        Action act = () => _ = new ActionAssociateFileV1Mapper(null!);

        // assert
        act.Should().ThrowExactly<ArgumentNullException>();
    }
    
   [Fact]
    public void CanMap_ShouldReturnFalse_WhenInputNull()
    {
        // arrange
        var sut = new ActionAssociateFileV1Mapper(_expressionEvaluator) as IActionToRepositoryActionMapper;

        // act
        var result = sut.CanMap(null!);

        // assert
        result.Should().BeFalse();
    }
    
    [Fact]
    public void CanMap_ShouldReturnFalse_WhenInputIsNotCorrectType()
    {
        // arrange

        var sut = new ActionAssociateFileV1Mapper(_expressionEvaluator) as IActionToRepositoryActionMapper;

        // act
        var result = sut.CanMap(new DummyRepositoryAction());

        // assert
        result.Should().BeFalse();
    }

    [Fact]
    public void CanMap_ShouldReturnTrue_WhenInputIsOfTypeRepositoryActionAssociateFileV1()
    {
        // arrange
        var sut = new ActionAssociateFileV1Mapper(_expressionEvaluator) as IActionToRepositoryActionMapper;

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
        var sut = new ActionAssociateFileV1Mapper(_expressionEvaluator) as IActionToRepositoryActionMapper;

        // act
        RepositoryActionBase[] result = sut.Map(action, _repository, ActionMapperCompositionFactory.CreateSmall(_expressionEvaluator, A.Dummy<IActionToRepositoryActionMapper>())).ToArray();

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
        var sut = new ActionAssociateFileV1Mapper(_expressionEvaluator) as IActionToRepositoryActionMapper;

        // act
        RepositoryActionBase[] result = sut.Map(action, _repository, ActionMapperCompositionFactory.CreateSmall(_expressionEvaluator, A.Dummy<IActionToRepositoryActionMapper>())).ToArray();

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
        var sut = new ActionAssociateFileV1Mapper(_expressionEvaluator) as IActionToRepositoryActionMapper;

        // act
        RepositoryActionBase[] result = sut.Map(action, _repository, ActionMapperCompositionFactory.CreateSmall(_expressionEvaluator, A.Dummy<IActionToRepositoryActionMapper>())).ToArray();

        // assert
        result.Should().BeEmpty();
    }
}

file class DummyRepositoryAction : Api.IO.ModuleBasedRepositoryActionProvider.Data.RepositoryAction
{
}