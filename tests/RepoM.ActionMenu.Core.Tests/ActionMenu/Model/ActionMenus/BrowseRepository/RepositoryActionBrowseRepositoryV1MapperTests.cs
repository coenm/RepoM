namespace RepoM.ActionMenu.Core.Tests.ActionMenu.Model.ActionMenus.BrowseRepository;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FakeItEasy;
using FluentAssertions;
using RepoM.ActionMenu.Core.ActionMenu.Model.ActionMenus.BrowseRepository;
using RepoM.ActionMenu.Interface.ActionMenuFactory;
using RepoM.ActionMenu.Interface.UserInterface;
using RepoM.ActionMenu.Interface.YamlModel;
using RepoM.Core.Plugin.Repository;
using VerifyTests;
using VerifyXunit;
using Xunit;

[UsesVerify]
public class RepositoryActionBrowseRepositoryV1MapperTests
{
    private readonly RepositoryActionBrowseRepositoryV1Mapper _sut = new ();
    private readonly RepositoryActionBrowseRepositoryV1 _action;
    private readonly IActionMenuGenerationContext _context;
    private readonly IRepository _repository;
    private readonly VerifySettings _verifySettings = new();

    private readonly List<Remote> _remotes = new()
      {
        new ("origin", "https://github.com/coenm/repom"),
      };

    public RepositoryActionBrowseRepositoryV1MapperTests()
    {
        _action = new RepositoryActionBrowseRepositoryV1()
            {
                Name = "dummy-name",
                FirstOnly = false,
            };
        _context = A.Fake<IActionMenuGenerationContext>();
        _repository = A.Fake<IRepository>();
        A.CallTo(() => _repository.Remotes).Returns(_remotes);
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
    public async Task Map_ShouldReturnEmpty_WhenRepoHasNoRemotes()
    {
        // arrange
        _remotes.Clear();

        // act
        List<UserInterfaceRepositoryActionBase> result = await _sut.MapAsync(_action, _context, _repository).ToListAsync();

        // assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task SingleRemote()
    {
        // arrange

        // act
        List<UserInterfaceRepositoryActionBase> result = await _sut.MapAsync(_action, _context, _repository).ToListAsync();

        // assert
        await Verifier.Verify(result, _verifySettings).IgnoreMembersWithType<IRepository>();
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Map_ShouldReturnSingle_WhenRepoHasSingleRemote(bool firstOnly)
    {
        // arrange
        _action.FirstOnly = firstOnly;

        // act
        List<UserInterfaceRepositoryActionBase> result = await _sut.MapAsync(_action, _context, _repository).ToListAsync();

        // assert
        await Verifier.Verify(result, _verifySettings).IgnoreMembersWithType<IRepository>().UseMethodName(nameof(SingleRemote));
    }

    [Fact]
    public async Task Map_ShouldReturnSingle_WhenRepoHasMultipleButWithFirstOnly()
    {
        // arrange
        _action.FirstOnly = true;
        _remotes.Add(new Remote("second", "https://second.com"));

        // act
        List<UserInterfaceRepositoryActionBase> result = await _sut.MapAsync(_action, _context, _repository).ToListAsync();

        // assert
        await Verifier.Verify(result, _verifySettings).IgnoreMembersWithType<IRepository>().UseMethodName(nameof(SingleRemote));
    }

    [Fact]
    public async Task Map_ShouldReturnDeferredSubActionsUserInterfaceRepositoryAction_WhenRepoHasMultiple()
    {
        // arrange
        _action.FirstOnly = false;
        _remotes.Add(new Remote("second", "https://second.com"));

        // act
        List<UserInterfaceRepositoryActionBase> result = await _sut.MapAsync(_action, _context, _repository).ToListAsync();

        // assert
        result.Should().HaveCount(1);
        result[0].Should().BeOfType<DeferredSubActionsUserInterfaceRepositoryAction>();
        await Verifier.Verify(result).IgnoreMembersWithType<IRepository>();
    }
}