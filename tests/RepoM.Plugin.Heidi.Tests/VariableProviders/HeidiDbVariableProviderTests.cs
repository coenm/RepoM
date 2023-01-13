namespace RepoM.Plugin.Heidi.Tests.VariableProviders;

using System;
using System.Collections.Immutable;
using System.Threading.Tasks;
using FakeItEasy;
using FluentAssertions;
using RepoM.Core.Plugin.Repository;
using RepoM.Plugin.Heidi.Interface;
using RepoM.Plugin.Heidi.Internal;
using RepoM.Plugin.Heidi.Internal.Config;
using RepoM.Plugin.Heidi.VariableProviders;
using VerifyTests;
using VerifyXunit;
using Xunit;

[UsesVerify]
public class HeidiDbVariableProviderTests
{
    private readonly IHeidiConfigurationService _configService;
    private readonly HeidiDbVariableProvider _sut;
    private readonly RepositoryContext _repositoryContext;
    private readonly IRepository _repository;
    private readonly VerifySettings _verifySettings;

    public HeidiDbVariableProviderTests()
    {
        _verifySettings = new VerifySettings();
        _verifySettings.UseDirectory("Verified");
        _repository = A.Fake<IRepository>();
        _repositoryContext = new RepositoryContext(_repository);
        _configService = A.Fake<IHeidiConfigurationService>();
        _sut = new HeidiDbVariableProvider(_configService);

        A.CallTo(() => _configService.GetAllDatabases())
         .Returns(new[]
             {
                 new HeidiSingleDatabaseConfiguration("RepoM/Abc")
                     {
                     },
             }.ToImmutableArray());

        A.CallTo(() => _configService.GetByRepository(_repository))
         .Returns(new RepositoryHeidiConfiguration[]
             {
                 new ("cc", 5, Array.Empty<string>(), "file1.txt", HeidiDbConfigFactory.CreateHeidiDbConfig(1)),
                 new ("bb", 1, new [] { "Test", }, "file1.txt", HeidiDbConfigFactory.CreateHeidiDbConfig(2)), // should be first in result, (order = 1)
                 new ("aa", 5, new [] { "Dev", }, "file1.txt", HeidiDbConfigFactory.CreateHeidiDbConfig(3)), // should be second in result, (order = 5, name aa < name cc)
             });
    }

    [Fact]
    public void Ctor_ShouldThrowArgumentNullException_WhenArgumentIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => new HeidiDbVariableProvider(null!));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("heididb")]
    [InlineData("heididb.")]
    [InlineData("heidi db")]
    [InlineData("heidi db.")]
    [InlineData(" heidi-db")]
    [InlineData(" heidi-db.")]
    public void CanProvide_ShouldReturnFalse_WhenKeyIsInvalid(string? key)
    {
        // arrange

        // act
        var result = _sut.CanProvide(key!);

        // assert
        result.Should().BeFalse();
    }

    [Theory]
    [InlineData("heidi-db.")]
    [InlineData("heidi-db.any")]
    [InlineData("heidi-DB.")]
    [InlineData("HeiDI-Db.")]
    [InlineData("HeiDI-Db.DBS")]
    public void CanProvide_ShouldReturnTrue_WhenKeyIsValid(string? key)
    {
        // arrange

        // act
        var result = _sut.CanProvide(key!);

        // assert
        result.Should().BeTrue();
    }

    [Fact]
    public void Provide_ShouldThrowNotImplementedException()
    {
        // arrange

        // act
        Action act = () => _sut.Provide("any", null);

        // assert
        act.Should().Throw<NotImplementedException>();
    }

    [Fact]
    public void Provide_ShouldReturnNull_WhenRepositoryIsNull()
    {
        // arrange

        // act
        var result = _sut.Provide(new RepositoryContext(new IRepository[] { null!, }), "heidi-db.repo.any", null);

        // assert
        result.Should().BeNull();
    }

    [Fact]
    public void Provide_ShouldReturnNull_WhenRepositoryContextIsEmpty()
    {
        // arrange

        // act
        var result = _sut.Provide(new RepositoryContext(Array.Empty<IRepository>()), "heidi-db.repo.any", null);

        // assert
        result.Should().BeNull();
    }

    [Theory]
    [InlineData("heidi-db.repo.DBS")]
    [InlineData("heidi-db.RePo.dBs")]
    [InlineData("heidi-db.repo.dbs")]
    public async Task Provide_ShouldReturnDatabases_WhenKeyHasValue(string key)
    {
        // arrange

        // act
        var result = _sut.Provide(_repositoryContext, key, null);

        // assert
        await Verifier.Verify(result, _verifySettings).IgnoreParametersForVerified(nameof(key));
    }

    [Theory]
    [InlineData("heidi-db.repo.count")]
    [InlineData("heidi-db.RePo.Count")]
    [InlineData("heidi-db.repo.Count")]
    public void Provide_ShouldReturnCount_WhenKeyIsCount(string key)
    {
        // arrange

        // act
        var result = _sut.Provide(_repositoryContext, key, null);

        // assert
        result.Should().BeOfType<int>().Which.Should().Be(3);
    }

    [Theory]
    [InlineData("heidi-db.repo.any")]
    [InlineData("heidi-db.RePo.Any")]
    [InlineData("heidi-db.repo.Any")]
    public void Provide_ShouldReturnTrue_WhenKeyIsAnyAndDatabasesFound(string key)
    {
        // arrange

        // act
        var result = _sut.Provide(_repositoryContext, key, null);

        // assert
        result.Should().BeOfType<bool>().Which.Should().BeTrue();
    }

    [Theory]
    [InlineData("heidi-db.repo.any", false)]
    [InlineData("heidi-db.RePo.Any", false)]
    [InlineData("heidi-db.repo.Any", false)]
    [InlineData("heidi-db.repo.empty", true)]
    [InlineData("heidi-db.RePo.Empty", true)]
    [InlineData("heidi-db.repo.Empty", true)]
    public void Provide_ShouldReturnFalse_WhenKeyIsAnyEmptyAndDatabasesNotFound(string key, bool expectedResult)
    {
        // arrange
        A.CallTo(() => _configService.GetByRepository(_repository)).Returns(Array.Empty<RepositoryHeidiConfiguration>());

        // act
        var result = _sut.Provide(_repositoryContext, key, null);

        // assert
        result.Should().BeOfType<bool>().Which.Should().Be(expectedResult);
    }
    
    [Theory]
    [InlineData("heidi-db.all.any", true)]
    [InlineData("heidi-db.ALL.Any", true)]
    [InlineData("heidi-db.all.Any", true)]
    [InlineData("heidi-db.all.empty", false)]
    [InlineData("heidi-db.ALL.Empty", false)]
    [InlineData("heidi-db.all.Empty", false)]
    public void Provide_ShouldReturnTrue_WhenKeyIsAllAnyEmptyAndDatabasesFound(string key, bool expectedResult)
    {
        // arrange

        // act
        var result = _sut.Provide(_repositoryContext, key, null);

        // assert
        result.Should().BeOfType<bool>().Which.Should().Be(expectedResult);
    }

    [Theory]
    [InlineData("heidi-db.all.any", false)]
    [InlineData("heidi-db.ALL.Any", false)]
    [InlineData("heidi-db.all.Any", false)]
    [InlineData("heidi-db.all.empty", true)]
    [InlineData("heidi-db.ALL.Empty", true)]
    [InlineData("heidi-db.all.Empty", true)]
    public void Provide_ShouldReturnFalse_WhenKeyIsAllAnyAndDatabasesNotFound(string key, bool expectedResult)
    {
        // arrange
        A.CallTo(() => _configService.GetAllDatabases()).Returns(Array.Empty<HeidiSingleDatabaseConfiguration>().ToImmutableArray());

        // act
        var result = _sut.Provide(_repositoryContext, key, null);

        // assert
        result.Should().BeOfType<bool>().Which.Should().Be(expectedResult);
    }

    [Fact]
    public void Provide_ShouldReturnNull_WhenKeyIsUnknown()
    {
        // arrange

        // act
        var result = _sut.Provide(_repositoryContext, "heidi-db.dummy", null);

        // assert
        result.Should().BeNull();
    }
}