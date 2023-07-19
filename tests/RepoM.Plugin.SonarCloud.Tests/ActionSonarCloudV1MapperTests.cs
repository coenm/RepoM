namespace RepoM.Plugin.SonarCloud.Tests;

using RepoM.Api.IO;
using System;
using System.ComponentModel.DataAnnotations;
using FakeItEasy;
using FluentAssertions;
using RepoM.Api.Common;
using RepoM.Api.IO.ModuleBasedRepositoryActionProvider.Data;
using RepoM.Core.Plugin.RepositoryFinder;
using Xunit;
using RepoM.Core.Plugin.Expressions;
using RepoM.Api.IO.ModuleBasedRepositoryActionProvider;

public class ActionSonarCloudV1MapperTests
{
    private readonly ISonarCloudFavoriteService _service;
    private readonly IRepositoryExpressionEvaluator _expressionEvaluator;
    private readonly ITranslationService _translationService;
    private readonly ActionSonarCloudV1Mapper _sut;

    public ActionSonarCloudV1MapperTests()
    {
        _service = A.Fake<ISonarCloudFavoriteService>();
        _expressionEvaluator = A.Fake<IRepositoryExpressionEvaluator>();
        _translationService = A.Fake<ITranslationService>();
        _sut = new ActionSonarCloudV1Mapper(_service, _expressionEvaluator, _translationService);
    }

    [Fact]
    public void Ctor_ShouldThrow_WhenArgumentNull()
    {
        // arrange
        ISonarCloudFavoriteService service = A.Fake<ISonarCloudFavoriteService>();
        IRepositoryExpressionEvaluator expressionEvaluator = A.Fake<IRepositoryExpressionEvaluator>();
        ITranslationService translationService = A.Fake<ITranslationService>();

        // act
        Func<ActionSonarCloudV1Mapper> act1 = () => new ActionSonarCloudV1Mapper(service, expressionEvaluator, null!);
        Func<ActionSonarCloudV1Mapper> act2 = () => new ActionSonarCloudV1Mapper(service, null!, translationService);
        Func<ActionSonarCloudV1Mapper> act3 = () => new ActionSonarCloudV1Mapper(null!, expressionEvaluator, translationService);

        // assert
        act1.Should().Throw<ArgumentNullException>();
        act2.Should().Throw<ArgumentNullException>();
        act3.Should().Throw<ArgumentNullException>();
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
    public void CanMap_ShouldReturnFalse_WhenWrongType()
    {
        // arrange

        // act
        var result = ((IActionToRepositoryActionMapper)_sut).CanMap(new RepositoryAction());

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