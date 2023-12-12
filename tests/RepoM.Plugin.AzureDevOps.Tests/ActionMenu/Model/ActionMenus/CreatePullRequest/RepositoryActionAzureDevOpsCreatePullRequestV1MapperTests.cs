namespace RepoM.Plugin.AzureDevOps.Tests.ActionMenu.Model.ActionMenus.CreatePullRequest;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FakeItEasy;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using RepoM.ActionMenu.Interface.ActionMenuFactory;
using RepoM.ActionMenu.Interface.UserInterface;
using RepoM.ActionMenu.Interface.YamlModel;
using RepoM.ActionMenu.Interface.YamlModel.Templating;
using RepoM.Core.Plugin.Repository;
using RepoM.Plugin.AzureDevOps.ActionMenu.Model.ActionMenus.CreatePullRequest;
using VerifyXunit;
using Xunit;

[UsesVerify]
public class RepositoryActionAzureDevOpsCreatePullRequestV1MapperTests
{
    private readonly RepositoryActionAzureDevOpsCreatePullRequestV1Mapper _sut = new (NullLogger.Instance);
    private readonly RepositoryActionAzureDevOpsCreatePullRequestV1 _action;
    private readonly IActionMenuGenerationContext _context;
    private readonly IRepository _repository;

    public RepositoryActionAzureDevOpsCreatePullRequestV1MapperTests()
    {
        _action = new RepositoryActionAzureDevOpsCreatePullRequestV1();
        _context = A.Fake<IActionMenuGenerationContext>();
        _repository = A.Fake<IRepository>();
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
    public void Map_ShouldReturnEmptySet_WhenWrongActionType()
    {
        // arrange

        // act
        var act = () => _ = _sut.MapAsync(A.Dummy<IMenuAction>(), _context, _repository);

        // assert
        act.Should().Throw<InvalidCastException>();
    }

    [Fact]
    public async Task Map_ShouldReturnEmpty_WhenRepositoryHasLocalChanges()
    {
        // arrange
        A.CallTo(() => _repository.HasLocalChanges).Returns(true);

        // act
        List<UserInterfaceRepositoryActionBase> result = await _sut.MapAsync(_action, _context, _repository).ToListAsync();

        // assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Map_ShouldReturnEmpty_WhenToBranchIsEmpty()
    {
        // arrange
        const string TO_BRANCH = "feature/abc-def";
        _action.ToBranch = TO_BRANCH;
        A.CallTo(() => _repository.HasLocalChanges).Returns(false);
        A.CallTo(() => _context.RenderStringAsync(TO_BRANCH)).ReturnsLazily(call => Task.FromResult(string.Empty));
        
        // act
        List<UserInterfaceRepositoryActionBase> result = await _sut.MapAsync(_action, _context, _repository).ToListAsync();

        // assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Map_ShouldReturnEmpty_WhenToBranchDoesNotExist()
    {
        // arrange
        const string TO_BRANCH = "feature/abc-def";
        _action.ToBranch = TO_BRANCH;
        A.CallTo(() => _repository.HasLocalChanges).Returns(false);
        A.CallTo(() => _repository.Branches).Returns(new[] { "feature/ABC-def", "feature/abc-defx", "main", });
        A.CallTo(() => _context.RenderStringAsync(A<string>._)).ReturnsLazily(call => Task.FromResult(string.Empty + call.Arguments[0]));
        
        // act
        List<UserInterfaceRepositoryActionBase> result = await _sut.MapAsync(_action, _context, _repository).ToListAsync();

        // assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Map_ShouldReturnEmpty_WhenToBranchIsSameAsCurrentBranch()
    {
        // arrange
        const string TO_BRANCH = "feature/abc-def";
        _action.ToBranch = TO_BRANCH;
        A.CallTo(() => _repository.HasLocalChanges).Returns(false);
        A.CallTo(() => _repository.Branches).Returns(new[] { "feature/abc-def", });
        A.CallTo(() => _repository.CurrentBranch).Returns(TO_BRANCH);
        A.CallTo(() => _context.RenderStringAsync((A<string>._))).ReturnsLazily(call => Task.FromResult(string.Empty + call.Arguments[0]));
        
        // act
        List<UserInterfaceRepositoryActionBase> result = await _sut.MapAsync(_action, _context, _repository).ToListAsync();

        // assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Map_ShouldReturnEmptySet_WhenProjectIdNotSet()
    {
        // arrange
        const string TO_BRANCH = "main";
        const string PROJECT_ID = "";
        _action.ToBranch = TO_BRANCH;
        _action.ProjectId = PROJECT_ID;
        A.CallTo(() => _repository.HasLocalChanges).Returns(false);
        A.CallTo(() => _repository.Branches).Returns(new[] { "main", });
        A.CallTo(() => _repository.CurrentBranch).Returns("feature/x");
        A.CallTo(() => _context.RenderStringAsync(A<string>._)).ReturnsLazily(call => Task.FromResult(string.Empty + call.Arguments[0]));
        
        // act
        List<UserInterfaceRepositoryActionBase> result = await _sut.MapAsync(_action, _context, _repository).ToListAsync();

        // assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Map_ShouldReturnResult_WhenAllOkayWithoutAutoComplete()
    {
        // arrange
        const string TO_BRANCH = "main";
        const string PROJECT_ID = "projId123";
        _action.ToBranch = TO_BRANCH;
        _action.ProjectId = PROJECT_ID;
        _action.DraftPr = "false";
        _action.IncludeWorkItems = "false";
        _action.OpenInBrowser = "true";
        _action.ReviewerIds = new List<Text> { "def", "abc", };
        _action.PrTitle = "pr title";
        _action.Name = "pr name";

        A.CallTo(() => _repository.HasLocalChanges).Returns(false);
        A.CallTo(() => _repository.Branches).Returns(new[] { "main", });
        A.CallTo(() => _repository.CurrentBranch).Returns("feature/x");
        A.CallTo(() => _context.RenderStringAsync(A<string>._)).ReturnsLazily(call => Task.FromResult("evaluated: " + call.Arguments[0]));
        A.CallTo(() => _context.RenderStringAsync(TO_BRANCH)).ReturnsLazily(call => Task.FromResult(TO_BRANCH));

        // act
        List<UserInterfaceRepositoryActionBase> result = await _sut.MapAsync(_action, _context, _repository).ToListAsync();

        // assert
        await Verifier.Verify(result).IgnoreMembersWithType<IRepository>();
    }

    [Fact]
    public async Task Map_ShouldReturnResult_WhenAllOkayWithAutoComplete()
    {
        // arrange
        const string TO_BRANCH = "main";
        const string PROJECT_ID = "projId123";
        _action.ToBranch = TO_BRANCH;
        _action.ProjectId = PROJECT_ID;
        _action.DraftPr = "false";
        _action.IncludeWorkItems = "false";
        _action.OpenInBrowser = "true";
        _action.ReviewerIds = new List<Text> { "def", "abc", };
        _action.PrTitle = "pr title";
        _action.Name = "pr name";
        _action.AutoComplete = new AutoCompleteOptionsV1
            {
                DeleteSourceBranch = "true",
                MergeStrategy = MergeStrategyV1.NoFastForward,
                TransitionWorkItems = "false",
            };

        A.CallTo(() => _repository.HasLocalChanges).Returns(false);
        A.CallTo(() => _repository.Branches).Returns(new[] { "main", });
        A.CallTo(() => _repository.CurrentBranch).Returns("feature/x");
        A.CallTo(() => _context.RenderStringAsync(A<string>._)).ReturnsLazily(call => Task.FromResult("evaluated: " + call.Arguments[0]));
        A.CallTo(() => _context.RenderStringAsync(TO_BRANCH)).ReturnsLazily(call => Task.FromResult(TO_BRANCH));

        // act
        List<UserInterfaceRepositoryActionBase> result = await _sut.MapAsync(_action, _context, _repository).ToListAsync();

        // assert
        await Verifier.Verify(result).IgnoreMembersWithType<IRepository>();
    }
}