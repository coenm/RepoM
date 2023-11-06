namespace RepoM.Plugin.Heidi.Tests.ActionMenu.Context;

using System;
using System.Collections;
using System.Threading.Tasks;
using FakeItEasy;
using FluentAssertions;
using RepoM.ActionMenu.Core.TestLib;
using RepoM.ActionMenu.Interface.ActionMenuFactory;
using RepoM.Core.Plugin.Repository;
using RepoM.Plugin.Heidi.ActionMenu.Context;
using RepoM.Plugin.Heidi.Interface;
using RepoM.Plugin.Heidi.Internal;
using VerifyXunit;
using Xunit;
using Xunit.Categories;

[UsesVerify]
public class HeidiDbVariablesTests
{
    private readonly IHeidiConfigurationService _service = A.Fake<IHeidiConfigurationService>();
    private readonly IActionMenuGenerationContext _context = A.Fake<IActionMenuGenerationContext>();
    private readonly HeidiDbVariables _sut;

    public HeidiDbVariablesTests()
    {
        A.CallTo(() => _context.Repository).Returns(A.Fake<IRepository>());

        // A.CallTo(() => _service.GetAllDatabases())
        //  .Returns(new[] { new HeidiSingleDatabaseConfiguration("RepoM/Abc"), }.ToImmutableArray());

        A.CallTo(() => _service.GetByRepository(_context.Repository))
         .Returns(new RepositoryHeidiConfiguration[]
             {
                 new ("cc", 5, Array.Empty<string>(), "file1.txt", HeidiDbConfigFactory.CreateHeidiDbConfig(1)),
                 new ("bb", 1, new [] { "Test", "Dev" }, "file1.txt", HeidiDbConfigFactory.CreateHeidiDbConfig(2)),
                 new ("aa", 5, new [] { "Dev", }, "file1.txt", HeidiDbConfigFactory.CreateHeidiDbConfig(3)),
             });

        _sut = new HeidiDbVariables(_service);
    }

    [Fact]
    public void Ctor_ShouldThrow_WhenArgumentNull()
    {
        // arrange

        // act
        Func<HeidiDbVariables> act = () => new HeidiDbVariables(null!);

        // assert
        act.Should().Throw<ArgumentNullException>();
    }
    
    [Fact]
    public void GetDatabases_ShouldCallService()
    {
        // arrange

        // act
        _ = _sut.GetDatabases(_context);

        // assert
        A.CallTo(() => _service.GetByRepository(_context.Repository)).MustHaveHappenedOnceExactly();
    }
    
    [Fact]
    public async Task GetDatabases_ShouldMapResult_WhenServiceReturnsDatabases()
    {
        // arrange

        // act
        IEnumerable result = _sut.GetDatabases(_context);

        // assert
        await Verifier.Verify(result);
    }
    
    [Fact]
    [Documentation]
    public async Task GetPullRequests_Documentation()
    {
        // arrange
        A.CallTo(() => _service.GetByRepository(_context.Repository))
         .Returns(new RepositoryHeidiConfiguration[]
             {
                 new (
                     "heidi-key",
                     1,
                     new [] { "Test", "Dev", },
                     "file1.txt",
                     new HeidiDbConfig
                         {
                             Comment = "HeidiSQL Comment",
                             Databases = new [] { "database1", "database2", },
                             Host = "database.my-domain.com",
                             Key = "MyDomainDb1",
                             WindowsAuth = false,
                             Library = "MSOLEDBSQL",
                             NetType = 1,
                             Password = "myS3cr3t!",
                             Port = 2345,
                             User = "coenm",
                         }),
             });

        // act
        IEnumerable result = _sut.GetDatabases(_context);

        // assert
        await DocumentationGeneration
              .CreateAndVerifyDocumentation(result)
              .UseFileName("heidi.databases");
    }
}