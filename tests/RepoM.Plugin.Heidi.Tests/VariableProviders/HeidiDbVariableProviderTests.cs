// namespace RepoM.Plugin.Heidi.Tests.VariableProviders;
//
// using System;
// using System.Collections.Immutable;
// using System.Threading.Tasks;
// using FakeItEasy;
// using FluentAssertions;
// using RepoM.Core.Plugin.Repository;
// using RepoM.Plugin.Heidi.Interface;
// using RepoM.Plugin.Heidi.Internal;
// using RepoM.Plugin.Heidi.Internal.Config;
// using VerifyTests;
// using VerifyXunit;
// using Xunit;
//
// [UsesVerify]
// public class HeidiDbVariableProviderTests
// {
//     private readonly IHeidiConfigurationService _configService;
//     private readonly HeidiDbVariableProvider _sut;
//     private readonly RepositoryContext _repositoryContext;
//     private readonly IRepository _repository;
//     private readonly VerifySettings _verifySettings;
//
//     public HeidiDbVariableProviderTests()
//     {
//         _verifySettings = new VerifySettings();
//         _verifySettings.UseDirectory("Verified");
//         _repository = A.Fake<IRepository>();
//         _repositoryContext = new RepositoryContext(_repository);
//         _configService = A.Fake<IHeidiConfigurationService>();
//         _sut = new HeidiDbVariableProvider(_configService);
//
//         A.CallTo(() => _configService.GetAllDatabases())
//          .Returns(new[] { new HeidiSingleDatabaseConfiguration("RepoM/Abc"), }.ToImmutableArray());
//
//         A.CallTo(() => _configService.GetByRepository(_repository))
//          .Returns(new RepositoryHeidiConfiguration[]
//              {
//                  new ("cc", 5, Array.Empty<string>(), "file1.txt", HeidiDbConfigFactory.CreateHeidiDbConfig(1)),
//                  new ("bb", 1, new [] { "Test", }, "file1.txt", HeidiDbConfigFactory.CreateHeidiDbConfig(2)), // should be first in result, (order = 1)
//                  new ("aa", 5, new [] { "Dev", }, "file1.txt", HeidiDbConfigFactory.CreateHeidiDbConfig(3)), // should be second in result, (order = 5, name aa < name cc)
//              });
//     }
//
//     [Fact]
//     public void Ctor_ShouldThrowArgumentNullException_WhenArgumentIsNull()
//     {
//         Assert.Throws<ArgumentNullException>(() => new HeidiDbVariableProvider(null!));
//     }
//     
//
//     [Theory]
//     [InlineData("heidi-db.repo.DBS")]
//     [InlineData("heidi-db.RePo.dBs")]
//     [InlineData("heidi-db.repo.dbs")]
//     public async Task Provide_ShouldReturnDatabases_WhenKeyHasValue(string key)
//     {
//         // arrange
//
//         // act
//         var result = _sut.Provide(_repositoryContext, key, null);
//
//         // assert
//         await Verifier.Verify(result, _verifySettings).IgnoreParametersForVerified(nameof(key));
//     }
//
//     [Theory]
//     [InlineData("heidi-db.repo.count")]
//     [InlineData("heidi-db.RePo.Count")]
//     [InlineData("heidi-db.repo.Count")]
//     public void Provide_ShouldReturnCount_WhenKeyIsCount(string key)
//     {
//         // arrange
//
//         // act
//         var result = _sut.Provide(_repositoryContext, key, null);
//
//         // assert
//         result.Should().BeOfType<int>().Which.Should().Be(3);
//     }
//
//     [Theory]
//     [InlineData("heidi-db.repo.any")]
//     [InlineData("heidi-db.RePo.Any")]
//     [InlineData("heidi-db.repo.Any")]
//     public void Provide_ShouldReturnTrue_WhenKeyIsAnyAndDatabasesFound(string key)
//     {
//         // arrange
//
//         // act
//         var result = _sut.Provide(_repositoryContext, key, null);
//
//         // assert
//         result.Should().BeOfType<bool>().Which.Should().BeTrue();
//     }
//
//     [Theory]
//     [InlineData("heidi-db.repo.any", false)]
//     [InlineData("heidi-db.RePo.Any", false)]
//     [InlineData("heidi-db.repo.Any", false)]
//     [InlineData("heidi-db.repo.empty", true)]
//     [InlineData("heidi-db.RePo.Empty", true)]
//     [InlineData("heidi-db.repo.Empty", true)]
//     public void Provide_ShouldReturnFalse_WhenKeyIsAnyEmptyAndDatabasesNotFound(string key, bool expectedResult)
//     {
//         // arrange
//         A.CallTo(() => _configService.GetByRepository(_repository)).Returns(Array.Empty<RepositoryHeidiConfiguration>());
//
//         // act
//         var result = _sut.Provide(_repositoryContext, key, null);
//
//         // assert
//         result.Should().BeOfType<bool>().Which.Should().Be(expectedResult);
//     }
//     
//     [Theory]
//     [InlineData("heidi-db.all.any", true)]
//     [InlineData("heidi-db.ALL.Any", true)]
//     [InlineData("heidi-db.all.Any", true)]
//     [InlineData("heidi-db.all.empty", false)]
//     [InlineData("heidi-db.ALL.Empty", false)]
//     [InlineData("heidi-db.all.Empty", false)]
//     public void Provide_ShouldReturnTrue_WhenKeyIsAllAnyEmptyAndDatabasesFound(string key, bool expectedResult)
//     {
//         // arrange
//
//         // act
//         var result = _sut.Provide(_repositoryContext, key, null);
//
//         // assert
//         result.Should().BeOfType<bool>().Which.Should().Be(expectedResult);
//     }
//
//     [Theory]
//     [InlineData("heidi-db.all.any", false)]
//     [InlineData("heidi-db.ALL.Any", false)]
//     [InlineData("heidi-db.all.Any", false)]
//     [InlineData("heidi-db.all.empty", true)]
//     [InlineData("heidi-db.ALL.Empty", true)]
//     [InlineData("heidi-db.all.Empty", true)]
//     public void Provide_ShouldReturnFalse_WhenKeyIsAllAnyAndDatabasesNotFound(string key, bool expectedResult)
//     {
//         // arrange
//         A.CallTo(() => _configService.GetAllDatabases()).Returns(Array.Empty<HeidiSingleDatabaseConfiguration>().ToImmutableArray());
//
//         // act
//         var result = _sut.Provide(_repositoryContext, key, null);
//
//         // assert
//         result.Should().BeOfType<bool>().Which.Should().Be(expectedResult);
//     }
//     
//     [Theory]
//     [InlineData("heidi-db.all.count")]
//     [InlineData("heidi-db.ALL.Count")]
//     [InlineData("heidi-db.all.Count")]
//     public void Provide_ShouldReturnZero_WhenKeyIsAllCountAndDatabasesIsEmpty(string key)
//     {
//         // arrange
//         A.CallTo(() => _configService.GetAllDatabases()).Returns(Array.Empty<HeidiSingleDatabaseConfiguration>().ToImmutableArray());
//
//         // act
//         var result = _sut.Provide(_repositoryContext, key, null);
//
//         // assert
//         result.Should().BeOfType<int>().Which.Should().Be(0);
//     }
//     
//     [Theory]
//     [InlineData("heidi-db.all.dbs")]
//     [InlineData("heidi-db.ALL.DbS")]
//     [InlineData("heidi-db.all.Dbs")]
//     public void Provide_ShouldReturnDatabases_WhenKeyIsAllDatabases(string key)
//     {
//         // arrange
//         var dbs = new HeidiSingleDatabaseConfiguration[] { new("test"), };
//         A.CallTo(() => _configService.GetAllDatabases()).Returns(dbs.ToImmutableArray());
//
//         // act
//         var result = _sut.Provide(_repositoryContext, key, null);
//
//         // assert
//         result.Should().BeOfType<HeidiSingleDatabaseConfiguration[]>().Which.Should().BeEquivalentTo(dbs);
//     }
//
//     [Theory]
//     [InlineData("heidi-db.dummy")]
//     [InlineData("heidi-db")]
//     [InlineData("heidi-db.all.dummy")]
//     [InlineData("heidi-db.repo.dummy")]
//     public void Provide_ShouldReturnNull_WhenKeyIsUnknown(string key)
//     {
//         // arrange
//
//         // act
//         var result = _sut.Provide(_repositoryContext, key, null);
//
//         // assert
//         result.Should().BeNull();
//     }
// }