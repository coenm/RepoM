namespace RepoM.Plugin.Clipboard.Tests.ActionMenu.Model.ActionMenus.ClipboardCopy;

using System;
using System.Linq;
using System.Threading.Tasks;
using FakeItEasy;
using FluentAssertions;
using RepoM.ActionMenu.Interface.ActionMenuFactory;
using RepoM.ActionMenu.Interface.YamlModel;
using RepoM.ActionMenu.Interface.YamlModel.Templating;
using RepoM.Core.Plugin.Repository;
using RepoM.Plugin.Clipboard.ActionMenu.Model.ActionMenus.ClipboardCopy;
using VerifyXunit;
using Xunit;

public class RepositoryActionClipboardCopyV1MapperTests
{
    private readonly RepositoryActionClipboardCopyV1Mapper _sut = new();
    private readonly RepositoryActionClipboardCopyV1 _action;
    private readonly IActionMenuGenerationContext _context;
    private readonly IRepository _repository;

    public RepositoryActionClipboardCopyV1MapperTests()
    {
        _action = new RepositoryActionClipboardCopyV1();
        _context = A.Fake<IActionMenuGenerationContext>();
        _repository = A.Fake<IRepository>();
        A.CallTo(() => _context.RenderStringAsync(A<string>._)).ReturnsLazily(call => Task.FromResult(call.Arguments[0] + "(evaluated)"));
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
    
    [Theory]
    [InlineData("text12313")]
    [InlineData("a b c2")]
    public async Task Map_ShouldMap_WhenTextSet(string text)
    {
        // arrange
        _action.Name = "name123";
        _action.Text = text;

        // act
        var result = await _sut.MapAsync(_action, _context, _repository).ToListAsync();

        // assert
        await Verifier.Verify(result).IgnoreMembersWithType<IRepository>().UseTextForParameters(text);
    }

    [Theory]
    [InlineData("name 1")]
    [InlineData("Name 2")]
    public async Task Map_ShouldMap_WhenNameSet(string name)
    {
        // arrange
        _action.Name = name;
        _action.Text = new Text();

        // act
        var result = await _sut.MapAsync(_action, _context, _repository).ToListAsync();

        // assert
        await Verifier.Verify(result).IgnoreMembersWithType<IRepository>().UseTextForParameters(name);
    }
}