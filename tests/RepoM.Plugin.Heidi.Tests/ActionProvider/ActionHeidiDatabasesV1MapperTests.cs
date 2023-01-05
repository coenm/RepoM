namespace RepoM.Plugin.Heidi.Tests.ActionProvider;

using System;
using System.Collections.Generic;
using System.Linq;
using FakeItEasy;
using FluentAssertions;
using LibGit2Sharp;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using RepoM.Api.Common;
using RepoM.Api.IO.ModuleBasedRepositoryActionProvider;
using RepoM.Api.IO.ModuleBasedRepositoryActionProvider.ActionMappers;
using RepoM.Core.Plugin.Expressions;
using RepoM.Plugin.Heidi.ActionProvider;
using RepoM.Plugin.Heidi.Interface;
using RepoM.Plugin.Heidi.Internal;
using Xunit;
using IRepository = RepoM.Core.Plugin.Repository.IRepository;
using Repository = RepoM.Api.Git.Repository;
using RepositoryAction = RepoM.Api.IO.ModuleBasedRepositoryActionProvider.Data.RepositoryAction;

public class ActionHeidiDatabasesV1MapperTests
{
    private IHeidiConfigurationService _service;
    private IRepositoryExpressionEvaluator _expressionEvaluator;
    private ITranslationService _translationService;
    private IHeidiSettings _settings;
    private ILogger _logger;
    private readonly ActionHeidiDatabasesV1Mapper _sut;
    private Repository _repository;
    private readonly RepositoryActionHeidiDatabasesV1 _action;
    private ActionMapperComposition _actionMapperComposition;

    public ActionHeidiDatabasesV1MapperTests()
    {
        _actionMapperComposition = new ActionMapperComposition(new List<IActionToRepositoryActionMapper>(), A.Dummy<IRepositoryExpressionEvaluator>());
        _repository = new Repository("dummy") { };
        _service = A.Fake<IHeidiConfigurationService>();
        _expressionEvaluator = A.Fake<IRepositoryExpressionEvaluator>();
        _translationService = A.Fake<ITranslationService>();
        _settings = A.Fake<IHeidiSettings>();
        _logger = NullLogger.Instance;

        _sut = new ActionHeidiDatabasesV1Mapper(_service, _expressionEvaluator, _translationService, _settings, _logger);

        _action = new RepositoryActionHeidiDatabasesV1
            {
                Active = "true",
                Name = "Abc",
                Executable = "heidi-test.exe",
                Key = null,
                MultiSelectEnabled = "false",
            };
        A.CallTo(() => _expressionEvaluator.EvaluateBooleanExpression("true", _repository)).Returns(true);
        A.CallTo(() => _expressionEvaluator.EvaluateBooleanExpression("false", _repository)).Returns(false);
        A.CallTo(() => _expressionEvaluator.EvaluateStringExpression(A<string>._, _repository))
         .ReturnsLazily(call =>
             {
                 if (call.Arguments.First() is string s)
                 {
                     return "ES_" + s;
                 }

                 throw new Exception("Thrown by test, Not expected");
             });
        A.CallTo(() => _service.GetByKey(A<string>._)).Returns(Array.Empty<HeidiConfiguration>());
        A.CallTo(() => _service.GetByRepository(_repository)).Returns(Array.Empty<HeidiConfiguration>());
    }

    [Fact]
    public void CanHandleMultipleRepositories_ShouldBeFalse()
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
        var result = _sut.CanMap(new RepositoryAction());

        // assert
        result.Should().BeFalse();
    }

    [Fact]
    public void CanMap_ShouldReturnFalse_WhenNull()
    {
        // arrange
        RepositoryActionHeidiDatabasesV1 action = null!;

        // act
        var result = _sut.CanMap(action);

        // assert
        result.Should().BeFalse();
    }

    [Fact]
    public void CanMap_ShouldReturnTrue_WhenRepositoryActionHeidiDatabasesV1()
    {
        // arrange
        var action = new RepositoryActionHeidiDatabasesV1();

        // act
        var result = _sut.CanMap(action);

        // assert
        result.Should().BeTrue();
    }

    [Fact]
    public void Map_ShouldReturnEmpty_WhenActionIsNull()
    {
        // arrange

        // act
        var result = _sut.Map(null!, new []{ _repository, }, _actionMapperComposition).ToArray();

        // assert
        result.Should().BeEmpty();
        A.CallTo(_service).MustNotHaveHappened();
    }

    [Fact]
    public void Map_ShouldReturnEmpty_WhenActionActiveIsFalse()
    {
        // arrange
        _action.Active = "false";

        // act
        var result = _sut.Map(_action, new []{ _repository, }, _actionMapperComposition).ToArray();

        // assert
        result.Should().BeEmpty();
        A.CallTo(_service).MustNotHaveHappened();
    }
    
    [Fact]
    public void Map_ShouldUseDefaultExe_WhenActionExecutableIsEmpty()
    {
        // arrange
        _action.Executable = string.Empty;

        // act
        _ = _sut.Map(_action, new[] { _repository, }, _actionMapperComposition).ToArray();

        // assert
        A.CallTo(() => _settings.DefaultExe).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public void Map_ShouldUseActionExeAndNotDefaultExe_WhenActionExecutableIsNotEmpty()
    {
        // arrange
        _action.Executable = "heidi123.exe";

        // act
        _ = _sut.Map(_action, new[] { _repository, }, _actionMapperComposition).ToArray();

        // assert
        A.CallTo(() => _expressionEvaluator.EvaluateStringExpression("heidi123.exe", _repository)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _settings.DefaultExe).MustNotHaveHappened();
    }

    [Fact]
    public void Map_ShouldReturnEmpty_WhenExecutableIsEmpty()
    {
        // arrange
        _action.Executable = string.Empty;
        A.CallTo(() => _settings.DefaultExe).Returns(string.Empty);

        // act
        var result = _sut.Map(_action, new []{ _repository, }, _actionMapperComposition).ToArray();

        // assert
        result.Should().BeEmpty();
        A.CallTo(_service).MustNotHaveHappened();
    }

    [Fact]
    public void Map_ShouldReturnEmpty_WhenExecutableIsEmptyAfterStringEvaluation()
    {
        // arrange
        _action.Executable = "test.exe";
        A.CallTo(() => _expressionEvaluator.EvaluateStringExpression("test.exe", _repository))
         .Returns(string.Empty);

        // act
        var result = _sut.Map(_action, new []{ _repository, }, _actionMapperComposition).ToArray();

        // assert
        result.Should().BeEmpty();
        A.CallTo(_service).MustNotHaveHappened();
    }

    [Fact]
    public void Map_ShouldQueryUsingKey_WhenKeyProvidedInAction()
    {
        // arrange
        _action.Key = "key1";

        // act
        _ = _sut.Map(_action, new []{ _repository, }, _actionMapperComposition).ToArray();

        // assert
        A.CallTo(() => _service.GetByKey("key1")).MustHaveHappenedOnceExactly();
        A.CallTo(() => _service.GetByRepository(A<IRepository>._)).MustNotHaveHappened();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Map_ShouldQueryUsingRepository_WhenKeyNotProvidedInAction(string? keyValue)
    {
        // arrange
        _action.Key = keyValue;

        // act
        _ = _sut.Map(_action, new []{ _repository, }, _actionMapperComposition).ToArray();

        // assert
        A.CallTo(() => _service.GetByRepository(_repository)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _service.GetByKey(A<string>._)).MustNotHaveHappened();
    }
}