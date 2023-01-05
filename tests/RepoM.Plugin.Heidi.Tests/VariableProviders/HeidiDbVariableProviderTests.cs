namespace RepoM.Plugin.Heidi.Tests.VariableProviders;

using System;
using System.Threading.Tasks;
using FakeItEasy;
using FluentAssertions;
using FluentAssertions.Common;
using RepoM.Core.Plugin.Repository;
using RepoM.Plugin.Heidi.Interface;
using RepoM.Plugin.Heidi.Internal;
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
    [InlineData("heidi db")]
    [InlineData(" heidi-db")]
    public void CanProvide_ShouldReturnFalse_WhenKeyIsInvalid(string? key)
    {
        // arrange

        // act
        var result = _sut.CanProvide(key!);

        // assert
        result.Should().BeFalse();
    }

    [Theory]
    [InlineData("heidi-db")]
    [InlineData("heidi-db.any")]
    [InlineData("heidi-DB")]
    [InlineData("HeiDI-Db")]
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
        var result = _sut.Provide(new RepositoryContext(new IRepository[] { null!, }), "heidi-db.any", null);

        // assert
        result.Should().BeNull();
    }

    [Fact]
    public void Provide_ShouldReturnNull_WhenRepositoryContextIsEmpty()
    {
        // arrange

        // act
        var result = _sut.Provide(new RepositoryContext(Array.Empty<IRepository>()), "heidi-db.any", null);

        // assert
        result.Should().BeNull();
    }

    [Theory]
    [InlineData("heidi-db")]
    [InlineData("heidi-db.dbs")]
    public async Task Provide_ShouldReturnDatabases_WhenKeyHasValue(string key)
    {
        // arrange
        A.CallTo(() => _configService.GetByRepository(_repository))
         .Returns(new HeidiConfiguration[]
            {
                new("cc", "bb1", 5, null),
                new ("bb", "bb2", 1, "Test"), // should be first in result, (order = 1)
                new ("aa", "bb3", 5, "dev"), // should be second in result, (order = 5, name aa < name cc)
            });

        // act
        var result = _sut.Provide(_repositoryContext, key, null);

        // assert
        await Verifier.Verify(result, _verifySettings).IgnoreParametersForVerified(nameof(key));
    }

    [Fact]
    public void Provide_ShouldReturnCount_WhenKeyIsCount()
    {
        // arrange
        A.CallTo(() => _configService.GetByRepository(_repository))
         .Returns(new HeidiConfiguration[]
             {
                 new("cc", "bb1", 5, null),
                 new ("bb", "bb2", 1, "Test"),
                 new ("aa", "bb3", 5, "dev"), 
             });

        // act
        var result = _sut.Provide(_repositoryContext, "heidi-db.count", null);

        // assert
        result.Should().BeOfType<int>().Which.Should().Be(3);
    }

    [Fact]
    public void Provide_ShouldReturnTrue_WhenKeyIsAnyAndDatabasesFound()
    {
        // arrange
        A.CallTo(() => _configService.GetByRepository(_repository))
         .Returns(new HeidiConfiguration[]
             {
                 new("cc", "bb1", 5, null),
                 new ("bb", "bb2", 1, "Test"),
                 new ("aa", "bb3", 5, "dev"), 
             });

        // act
        var result = _sut.Provide(_repositoryContext, "heidi-db.any", null);

        // assert
        result.Should().BeOfType<bool>().Which.Should().BeTrue();
    }

    [Fact]
    public void Provide_ShouldReturnFalse_WhenKeyIsAnyAndDatabasesNotFound()
    {
        // arrange
        A.CallTo(() => _configService.GetByRepository(_repository)).Returns(Array.Empty<HeidiConfiguration>());

        // act
        var result = _sut.Provide(_repositoryContext, "heidi-db.any", null);

        // assert
        result.Should().BeOfType<bool>().Which.Should().BeFalse();
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