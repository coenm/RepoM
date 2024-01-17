namespace RepoM.Plugin.SonarCloud.Tests.ActionMenu.Model.ActionMenus.SetFavorite;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FakeItEasy;
using FluentAssertions;
using RepoM.ActionMenu.Interface.ActionMenuFactory;
using RepoM.ActionMenu.Interface.UserInterface;
using RepoM.ActionMenu.Interface.YamlModel;
using RepoM.ActionMenu.Interface.YamlModel.Templating;
using RepoM.Core.Plugin.Repository;
using RepoM.Plugin.SonarCloud;
using RepoM.Plugin.SonarCloud.ActionMenu.Model.ActionMenus.SetFavorite;
using VerifyXunit;
using Xunit;
using RepositoryActionSonarCloudSetFavoriteV1 = RepoM.Plugin.SonarCloud.ActionMenu.Model.ActionMenus.SetFavorite.RepositoryActionSonarCloudSetFavoriteV1;

[UsesVerify]
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
        _sut = new(_service);
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

    //
    // [Fact]
    // public async Task Map_ShouldReturnEmpty_WhenToBranchIsEmpty()
    // {
    //     // arrange
    //     const string TO_BRANCH = "feature/abc-def";
    //     _action.ToBranch = TO_BRANCH;
    //     A.CallTo(() => _repository.HasLocalChanges).Returns(false);
    //     A.CallTo(() => _context.RenderStringAsync(TO_BRANCH)).ReturnsLazily(call => Task.FromResult(string.Empty));
    //     
    //     // act
    //     List<UserInterfaceRepositoryActionBase> result = await _sut.MapAsync(_action, _context, _repository).ToListAsync();
    //
    //     // assert
    //     result.Should().BeEmpty();
    // }
    //
    // [Fact]
    // public async Task Map_ShouldReturnEmpty_WhenToBranchDoesNotExist()
    // {
    //     // arrange
    //     const string TO_BRANCH = "feature/abc-def";
    //     _action.ToBranch = TO_BRANCH;
    //     A.CallTo(() => _repository.HasLocalChanges).Returns(false);
    //     A.CallTo(() => _repository.Branches).Returns(new[] { "feature/ABC-def", "feature/abc-defx", "main", });
    //     A.CallTo(() => _context.RenderStringAsync(A<string>._)).ReturnsLazily(call => Task.FromResult(string.Empty + call.Arguments[0]));
    //     
    //     // act
    //     List<UserInterfaceRepositoryActionBase> result = await _sut.MapAsync(_action, _context, _repository).ToListAsync();
    //
    //     // assert
    //     result.Should().BeEmpty();
    // }
    //
    // [Fact]
    // public async Task Map_ShouldReturnEmpty_WhenToBranchIsSameAsCurrentBranch()
    // {
    //     // arrange
    //     const string TO_BRANCH = "feature/abc-def";
    //     _action.ToBranch = TO_BRANCH;
    //     A.CallTo(() => _repository.HasLocalChanges).Returns(false);
    //     A.CallTo(() => _repository.Branches).Returns(new[] { "feature/abc-def", });
    //     A.CallTo(() => _repository.CurrentBranch).Returns(TO_BRANCH);
    //     A.CallTo(() => _context.RenderStringAsync((A<string>._))).ReturnsLazily(call => Task.FromResult(string.Empty + call.Arguments[0]));
    //     
    //     // act
    //     List<UserInterfaceRepositoryActionBase> result = await _sut.MapAsync(_action, _context, _repository).ToListAsync();
    //
    //     // assert
    //     result.Should().BeEmpty();
    // }
    //
    // [Fact]
    // public async Task Map_ShouldReturnEmptySet_WhenProjectIdNotSet()
    // {
    //     // arrange
    //     const string TO_BRANCH = "main";
    //     const string PROJECT_ID = "";
    //     _action.ToBranch = TO_BRANCH;
    //     _action.ProjectId = PROJECT_ID;
    //     A.CallTo(() => _repository.HasLocalChanges).Returns(false);
    //     A.CallTo(() => _repository.Branches).Returns(new[] { "main", });
    //     A.CallTo(() => _repository.CurrentBranch).Returns("feature/x");
    //     A.CallTo(() => _context.RenderStringAsync(A<string>._)).ReturnsLazily(call => Task.FromResult(string.Empty + call.Arguments[0]));
    //     
    //     // act
    //     List<UserInterfaceRepositoryActionBase> result = await _sut.MapAsync(_action, _context, _repository).ToListAsync();
    //
    //     // assert
    //     result.Should().BeEmpty();
    // }
    //
    // [Fact]
    // public async Task Map_ShouldReturnResult_WhenAllOkayWithoutAutoComplete()
    // {
    //     // arrange
    //     const string TO_BRANCH = "main";
    //     const string PROJECT_ID = "projId123";
    //     _action.ToBranch = TO_BRANCH;
    //     _action.ProjectId = PROJECT_ID;
    //     _action.DraftPr = "false";
    //     _action.IncludeWorkItems = "false";
    //     _action.OpenInBrowser = "true";
    //     _action.ReviewerIds = new List<Text> { "def", "abc", };
    //     _action.PrTitle = "pr title";
    //     _action.Name = "pr name";
    //
    //     A.CallTo(() => _repository.HasLocalChanges).Returns(false);
    //     A.CallTo(() => _repository.Branches).Returns(new[] { "main", });
    //     A.CallTo(() => _repository.CurrentBranch).Returns("feature/x");
    //     A.CallTo(() => _context.RenderStringAsync(A<string>._)).ReturnsLazily(call => Task.FromResult("evaluated: " + call.Arguments[0]));
    //     A.CallTo(() => _context.RenderStringAsync(TO_BRANCH)).ReturnsLazily(call => Task.FromResult(TO_BRANCH));
    //
    //     // act
    //     List<UserInterfaceRepositoryActionBase> result = await _sut.MapAsync(_action, _context, _repository).ToListAsync();
    //
    //     // assert
    //     await Verifier.Verify(result).IgnoreMembersWithType<IRepository>();
    // }
    //
    // [Fact]
    // public async Task Map_ShouldReturnResult_WhenAllOkayWithAutoComplete()
    // {
    //     // arrange
    //     const string TO_BRANCH = "main";
    //     const string PROJECT_ID = "projId123";
    //     _action.ToBranch = TO_BRANCH;
    //     _action.ProjectId = PROJECT_ID;
    //     _action.DraftPr = "false";
    //     _action.IncludeWorkItems = "false";
    //     _action.OpenInBrowser = "true";
    //     _action.ReviewerIds = new List<Text> { "def", "abc", };
    //     _action.PrTitle = "pr title";
    //     _action.Name = "pr name";
    //     _action.AutoComplete = new AutoCompleteOptionsV1
    //         {
    //             DeleteSourceBranch = "true",
    //             MergeStrategy = MergeStrategyV1.NoFastForward,
    //             TransitionWorkItems = "false",
    //         };
    //
    //     A.CallTo(() => _repository.HasLocalChanges).Returns(false);
    //     A.CallTo(() => _repository.Branches).Returns(new[] { "main", });
    //     A.CallTo(() => _repository.CurrentBranch).Returns("feature/x");
    //     A.CallTo(() => _context.RenderStringAsync(A<string>._)).ReturnsLazily(call => Task.FromResult("evaluated: " + call.Arguments[0]));
    //     A.CallTo(() => _context.RenderStringAsync(TO_BRANCH)).ReturnsLazily(call => Task.FromResult(TO_BRANCH));
    //
    //     // act
    //     List<UserInterfaceRepositoryActionBase> result = await _sut.MapAsync(_action, _context, _repository).ToListAsync();
    //
    //     // assert
    //     await Verifier.Verify(result).IgnoreMembersWithType<IRepository>();
    // }
}