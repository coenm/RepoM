namespace RepoM.Plugin.SonarCloud.Tests.ActionMenu.Model.ActionMenus.SetFavorite;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FakeItEasy;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using RepoM.ActionMenu.Interface.ActionMenuFactory;
using RepoM.ActionMenu.Interface.UserInterface;
using RepoM.ActionMenu.Interface.YamlModel;
using RepoM.Core.Plugin.Repository;
using RepoM.Plugin.SonarCloud;
using RepoM.Plugin.SonarCloud.ActionMenu.Model.ActionMenus.SetFavorite;
using VerifyXunit;
using Xunit;
using RepositoryActionSonarCloudSetFavoriteV1 = RepoM.Plugin.SonarCloud.ActionMenu.Model.ActionMenus.SetFavorite.RepositoryActionSonarCloudSetFavoriteV1;

public class RepositoryActionSonarCloudSetFavoriteV1MapperTests
{
    private readonly RepositoryActionSonarCloudSetFavoriteV1Mapper _sut;
    private readonly RepositoryActionSonarCloudSetFavoriteV1 _action;
    private readonly IActionMenuGenerationContext _context;
    private readonly IRepository _repository;
    private readonly ISonarCloudFavoriteService _service;

    public RepositoryActionSonarCloudSetFavoriteV1MapperTests()
    {
        _action = new RepositoryActionSonarCloudSetFavoriteV1
            {
                Active = true,
                Name = "menu name",
                Project = "proj-abc",
            };
        _context = A.Fake<IActionMenuGenerationContext>();
        _repository = A.Fake<IRepository>();
        A.CallTo(() => _context.RenderStringAsync(A<string>._)).ReturnsLazily(call => Task.FromResult(call.Arguments[0] + "(evaluated)"));

        _service = A.Fake<ISonarCloudFavoriteService>();
        A.CallTo(() => _service.IsInitialized).Returns(true);
        _sut = new(_service, A.Dummy<ILogger>());
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
    public async Task Map_ShouldReturnEmpty_WhenNotInitialized()
    {
        // arrange
        A.CallTo(() => _service.IsInitialized).Returns(false);

        // act
        List<UserInterfaceRepositoryActionBase> result = await _sut.MapAsync(_action, _context, _repository).ToListAsync();
    
        // assert
        result.Should().BeEmpty();
        A.CallTo(_service).MustHaveHappenedOnceExactly();
        A.CallTo(() => _service.IsInitialized).MustHaveHappenedOnceExactly();
    }
    
    [Fact]
    public async Task Map_ShouldReturnAsExpected_WhenInitialized()
    {
        // arrange
        A.CallTo(() => _service.IsInitialized).Returns(true);

        // act
        List<UserInterfaceRepositoryActionBase> result = await _sut.MapAsync(_action, _context, _repository).ToListAsync();

        // assert
        await Verifier.Verify(result).IgnoreMembersWithType<IRepository>();
    }
}