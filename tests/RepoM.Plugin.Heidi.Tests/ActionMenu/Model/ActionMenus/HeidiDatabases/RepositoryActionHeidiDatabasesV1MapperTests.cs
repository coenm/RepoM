namespace RepoM.Plugin.Heidi.Tests.ActionMenu.Model.ActionMenus.HeidiDatabases;

using System;
using System.Linq;
using System.Threading.Tasks;
using FakeItEasy;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using RepoM.ActionMenu.Interface.ActionMenuFactory;
using RepoM.ActionMenu.Interface.YamlModel;
using RepoM.Core.Plugin.Repository;
using RepoM.Plugin.Heidi.ActionMenu.Model.ActionMenus.HeidiDatabases;
using RepoM.Plugin.Heidi.Interface;
using RepoM.Plugin.Heidi.Internal;
using VerifyXunit;
using Xunit;

[UsesVerify]
public class RepositoryActionHeidiDatabasesV1MapperTests
{
    private readonly RepositoryActionHeidiDatabasesV1Mapper _sut;
    private readonly RepositoryActionHeidiDatabasesV1 _action;
    private readonly IActionMenuGenerationContext _context;
    private readonly IRepository _repository;

    private readonly IHeidiConfigurationService _service;
    private readonly IHeidiSettings _settings;

    public RepositoryActionHeidiDatabasesV1MapperTests()
    {
        _action = new RepositoryActionHeidiDatabasesV1
            {
                Executable = "dummy.exe",
                Name = "Dummy Name",
            };
        _context = A.Fake<IActionMenuGenerationContext>();
        _repository = A.Fake<IRepository>();
        A.CallTo(() => _context.RenderStringAsync(A<string>._)).ReturnsLazily(call => Task.FromResult(call.Arguments[0] + "(evaluated)"));

        _service = A.Fake<IHeidiConfigurationService>();
        _settings = A.Fake<IHeidiSettings>();
        _sut = new RepositoryActionHeidiDatabasesV1Mapper(_service, _settings, NullLogger.Instance);
        A.CallTo(() => _service.GetByKey(A<string>._)).Returns(Array.Empty<RepositoryHeidiConfiguration>());
        A.CallTo(() => _service.GetByRepository(_repository)).Returns(Array.Empty<RepositoryHeidiConfiguration>());
    }

    [Fact]
    public void CanMap_ShouldReturnTrue_WhenTypeIsCorrect()
    {
        _sut.CanMap(_action).Should().BeTrue();
    }

    [Fact]
    public void CanMap_ShouldReturnFalse_WhenTypeIsIncorrect()
    {
        _sut.CanMap(A.Dummy<IMenuAction>()).Should().BeFalse();
    }

    [Fact]
    public void Map_ShouldThrow_WhenWrongActionType()
    {
        // arrange

        // act
        var act = () => _ = _sut.MapAsync(A.Dummy<IMenuAction>(), _context, _repository);

        // assert
        act.Should().Throw<InvalidCastException>();
    }


    [Fact]
    public async Task Map_ShouldUseDefaultExe_WhenActionExecutableIsEmpty()
    {
        // arrange
        _action.Executable = "dummy-exe.exe";
        A.CallTo(() => _context.RenderStringAsync("dummy-exe.exe")).ReturnsLazily(_ => Task.FromResult(string.Empty));

        // act
        _ = await _sut.MapAsync(_action, _context, _repository).ToListAsync();

        // assert
        A.CallTo(() => _settings.DefaultExe).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Map_ShouldUseActionExeAndNotDefaultExe_WhenActionExecutableIsNotEmpty()
    {
        // arrange
        _action.Executable = "heidi123.exe";

        // act
        _ = await _sut.MapAsync(_action, _context, _repository).ToListAsync();

        // assert
        A.CallTo(() => _settings.DefaultExe).MustNotHaveHappened();
    }

    [Fact]
    public async Task Map_ShouldReturnEmpty_WhenExecutableIsEmpty()
    {
        // arrange
        _action.Executable = "dummy-exe.exe";
        A.CallTo(() => _context.RenderStringAsync("dummy-exe.exe")).ReturnsLazily(_ => Task.FromResult(string.Empty));
        A.CallTo(() => _settings.DefaultExe).Returns(string.Empty);

        // act
        var result = await _sut.MapAsync(_action, _context, _repository).ToListAsync();

        // assert
        result.Should().BeEmpty();
        A.CallTo(_service).MustNotHaveHappened();
    }

    [Fact]
    public async Task Map_ShouldReturnEmpty_WhenExecutableIsEmptyAfterStringEvaluation()
    {
        // arrange
        _action.Executable = "test.exe";
        A.CallTo(() => _context.RenderStringAsync("test.exe")).ReturnsLazily(_ => Task.FromResult(string.Empty));

        // act
        var result = await _sut.MapAsync(_action, _context, _repository).ToListAsync();

        // assert
        result.Should().BeEmpty();
        A.CallTo(_service).MustNotHaveHappened();
    }

    [Fact]
    public async Task Map_ShouldQueryUsingKey_WhenKeyProvidedInAction()
    {
        // arrange
        _action.Key = "key1";

        // act
        _ = await _sut.MapAsync(_action, _context, _repository).ToListAsync();

        // assert
        A.CallTo(() => _service.GetByKey("key1(evaluated)")).MustHaveHappenedOnceExactly();
        A.CallTo(() => _service.GetByRepository(A<IRepository>._)).MustNotHaveHappened();
    }

    [Fact]
    public async Task Map_ShouldQueryUsingRepository_WhenKeyNotProvidedInAction()
    {
        // arrange
        _action.Key = "dummy-key";
        A.CallTo(() => _context.RenderStringAsync("dummy-key")).ReturnsLazily(_ => Task.FromResult(string.Empty));

        // act
        _ = await _sut.MapAsync(_action, _context, _repository).ToListAsync();

        // assert
        A.CallTo(() => _service.GetByRepository(_repository)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _service.GetByKey(A<string>._)).MustNotHaveHappened();
    }

    [Fact]
    public async Task Map_ShouldReturnNoDatabasesFoundAction_WhenNameIsNullOrEmptyAndNoDatabasesFound()
    {
        // arrange
        _action.Name = string.Empty;

        // act
        var result = await _sut.MapAsync(_action, _context, _repository).ToListAsync();

        // assert
        await Verifier.Verify(result).IgnoreMembersWithType<IRepository>();
    }

    [Fact]
    public async Task Map_ShouldReturnActions_WhenNameIsNullOrEmptyAndDatabasesFound()
    {
        // arrange
        _action.Name = string.Empty;
        A.CallTo(() => _service.GetByRepository(_repository))
         .Returns(new RepositoryHeidiConfiguration[]
             {
                 new ("A-name", 6, Array.Empty<string>(), "file1.txt", HeidiDbConfigFactory.CreateHeidiDbConfig(1, "B-description")),
                 new ("D-name", 2, Array.Empty<string>(), "file1.txt", HeidiDbConfigFactory.CreateHeidiDbConfig(2, "E-description")),
             });

        // act
        var result = await _sut.MapAsync(_action, _context, _repository).ToListAsync();

        // assert
        await Verifier.Verify(result).IgnoreMembersWithType<IRepository>();
    }

    [Fact]
    public async Task Map_ShouldReturnFolderWithNoDatabasesFoundAction_WhenNameIsSetAndNoDatabasesFound()
    {
        // arrange
        _action.Name = "Databases";

        // act
        var result = await _sut.MapAsync(_action, _context, _repository).ToListAsync();

        // assert
        await Verifier.Verify(result).IgnoreMembersWithType<IRepository>();
    }

    [Fact]
    public async Task Map_ShouldReturnFolderWithDatabaseActions_WhenNameIsSetAndDatabasesFound()
    {
        // arrange
        _action.Name = "Databases";
        A.CallTo(() => _service.GetByRepository(_repository))
         .Returns(new RepositoryHeidiConfiguration[]
             {
                 new ("A-name", 6, Array.Empty<string>(), "file1.txt", HeidiDbConfigFactory.CreateHeidiDbConfig(1, "B-description")),
                 new ("D-name", 2, Array.Empty<string>(), "file1.txt", HeidiDbConfigFactory.CreateHeidiDbConfig(2, "E-description")),
             });

        // act
        var result = await _sut.MapAsync(_action, _context, _repository).ToListAsync();

        // assert
        await Verifier.Verify(result).IgnoreMembersWithType<IRepository>();
    }
}