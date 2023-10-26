namespace RepoM.Plugin.AzureDevOps.Tests.ActionMenu.Model.ActionMenus.CreatePullRequest;

using FakeItEasy;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using RepoM.ActionMenu.Interface.YamlModel;
using RepoM.Plugin.AzureDevOps.ActionMenu.Model.ActionMenus.CreatePullRequest;
using Xunit;

public class RepositoryActionAzureDevOpsCreatePullRequestV1MapperTests
{
    private readonly RepositoryActionAzureDevOpsCreatePullRequestV1 _action;
    private readonly RepositoryActionAzureDevOpsCreatePullRequestV1Mapper _sut = new RepositoryActionAzureDevOpsCreatePullRequestV1Mapper(NullLogger.Instance);

    public RepositoryActionAzureDevOpsCreatePullRequestV1MapperTests()
    {
        _action = new RepositoryActionAzureDevOpsCreatePullRequestV1();
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
}