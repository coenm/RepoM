namespace RepoM.Plugin.Heidi.Tests.ActionProvider;

using System.IO.Abstractions;
using FakeItEasy;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using RepoM.Api.Common;
using RepoM.Api.IO.ModuleBasedRepositoryActionProvider.Data;
using RepoM.Core.Plugin.Expressions;
using RepoM.Plugin.Heidi.ActionProvider;
using RepoM.Plugin.Heidi.Internal;
using Xunit;

public class ActionHeidiDatabasesV1MapperTests
{
    private IHeidiConfigurationService _service;
    private IRepositoryExpressionEvaluator _expressionEvaluator;
    private ITranslationService _translationService;
    private IFileSystem _fileSystem;
    private IHeidiSettings _settings;
    private ILogger _logger;
    private readonly ActionHeidiDatabasesV1Mapper _sut;

    public ActionHeidiDatabasesV1MapperTests()
    {
        _service = A.Fake<IHeidiConfigurationService>();
        _expressionEvaluator = A.Fake<IRepositoryExpressionEvaluator>();
        _translationService = A.Fake<ITranslationService>();
        _fileSystem = A.Fake<IFileSystem>();
        _settings = A.Fake<IHeidiSettings>();
        _logger = NullLogger.Instance;

        _sut = new ActionHeidiDatabasesV1Mapper(_service, _expressionEvaluator, _translationService, _fileSystem, _settings, _logger);
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
}