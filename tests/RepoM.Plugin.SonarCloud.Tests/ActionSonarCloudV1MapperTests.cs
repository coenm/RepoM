namespace RepoM.Plugin.SonarCloud.Tests;

using System;
using FakeItEasy;
using FluentAssertions;
using RepoM.Api.IO.ModuleBasedRepositoryActionProvider.Data;
using Xunit;
using RepoM.Core.Plugin.Expressions;
using RepoM.Api.IO.ModuleBasedRepositoryActionProvider;

public class ActionSonarCloudV1MapperTests
{
    private readonly ISonarCloudFavoriteService _service;
    private readonly IRepositoryExpressionEvaluator _expressionEvaluator;
    private readonly ActionSonarCloudV1Mapper _sut;

    public ActionSonarCloudV1MapperTests()
    {
        _service = A.Fake<ISonarCloudFavoriteService>();
        _expressionEvaluator = A.Fake<IRepositoryExpressionEvaluator>();
        _sut = new ActionSonarCloudV1Mapper(_service, _expressionEvaluator);
    }

    [Fact]
    public void Ctor_ShouldThrow_WhenArgumentNull()
    {
        // arrange
        ISonarCloudFavoriteService service = A.Fake<ISonarCloudFavoriteService>();
        IRepositoryExpressionEvaluator expressionEvaluator = A.Fake<IRepositoryExpressionEvaluator>();

        // act
        Func<ActionSonarCloudV1Mapper> act1 = () => new ActionSonarCloudV1Mapper(service, null!);
        Func<ActionSonarCloudV1Mapper> act2 = () => new ActionSonarCloudV1Mapper(null!, expressionEvaluator);

        // assert
        act1.Should().Throw<ArgumentNullException>();
        act2.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void CanMap_ShouldReturnFalse_WhenWrongType()
    {
        // arrange

        // act
        var result = ((IActionToRepositoryActionMapper)_sut).CanMap(new DummyRepositoryAction());

        // assert
        result.Should().BeFalse();
    }

    [Fact]
    public void CanMap_ShouldReturnFalse_WhenNull()
    {
        // arrange
        RepositoryActionSonarCloudSetFavoriteV1 action = null!;

        // act
        var result = ((IActionToRepositoryActionMapper)_sut).CanMap(action);

        // assert
        result.Should().BeFalse();
    }

    [Fact]
    public void CanMap_ShouldReturnTrue_WhenRepositoryActionSonarCloudSetFavoriteV1()
    {
        // arrange
        var action = new RepositoryActionSonarCloudSetFavoriteV1();

        // act
        var result = ((IActionToRepositoryActionMapper)_sut).CanMap(action);

        // assert
        result.Should().BeTrue();
    }
}

file class DummyRepositoryAction : RepositoryAction
{
}