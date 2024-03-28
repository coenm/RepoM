namespace RepoM.ActionMenu.Core.Tests.ActionMenu.Model.ActionMenus.Command;

using System;
using System.Linq;
using System.Threading.Tasks;
using FakeItEasy;
using FluentAssertions;
using RepoM.ActionMenu.Core.ActionMenu.Model.ActionMenus.Command;
using RepoM.ActionMenu.Interface.ActionMenuFactory;
using RepoM.ActionMenu.Interface.YamlModel;
using RepoM.Core.Plugin.Repository;
using VerifyTests;
using VerifyXunit;
using Xunit;

public class RepositoryActionCommandV1MapperTests
{
    private readonly RepositoryActionCommandV1Mapper _sut = new ();
    private readonly RepositoryActionCommandV1 _action;
    private readonly IActionMenuGenerationContext _context;
    private readonly IRepository _repository;
    private readonly VerifySettings _verifySettings = new();

    public RepositoryActionCommandV1MapperTests()
    {
        _action = new RepositoryActionCommandV1
            {
                Name = "dummy-name",
                Command = "abc.exe",
                Arguments = "a b x",
            };
        _context = A.Fake<IActionMenuGenerationContext>();
        _repository = A.Fake<IRepository>();
        A.CallTo(() => _context.RenderStringAsync(A<string>._)).ReturnsLazily(call => Task.FromResult(call.Arguments[0] + "(evaluated)"));

        _verifySettings.DisableRequireUniquePrefix();
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
    public async Task Map_ShouldReturnItem_WhenSuccess()
    {
        // arrange
    
        // act
        var result = await _sut.MapAsync(_action, _context, _repository).ToListAsync();
    
        // assert
        await Verifier.Verify(result, _verifySettings).IgnoreMembersWithType<IRepository>();
    }   
}