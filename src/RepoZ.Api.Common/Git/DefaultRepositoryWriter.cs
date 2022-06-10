namespace RepoZ.Api.Common.Git;

using System;
using System.Linq;
using LibGit2Sharp;
using RepoM.Api.Git;
using RepoZ.Api.Common.Common;
using Repository = RepoM.Api.Git.Repository;

public class DefaultRepositoryWriter : IRepositoryWriter
{
    private readonly IGitCommander _gitCommander;
    private readonly IAppSettingsService _appSettingsService;

    public DefaultRepositoryWriter(IGitCommander gitCommander, IAppSettingsService appSettingsService)
    {
        _gitCommander = gitCommander ?? throw new ArgumentNullException(nameof(gitCommander));
        _appSettingsService = appSettingsService ?? throw new ArgumentNullException(nameof(appSettingsService));
    }

    public bool Checkout(Repository repository, string branchName)
    {
        using var repo = new LibGit2Sharp.Repository(repository.Path);
        Branch branch;

        // Check if local branch exists
        if (repo.Branches.Any(b => b.FriendlyName == branchName))
        {
            branch = Commands.Checkout(repo, branchName);
        }
        else
        {
            // Create local branch to remote branch tip and set its upstream branch to remote
            Branch? upstreamBranch = repo.Branches.FirstOrDefault(b => string.Equals(b.UpstreamBranchCanonicalName, "refs/heads/" + branchName, StringComparison.OrdinalIgnoreCase));

            if (upstreamBranch is null)
            {
                return false;
            }

            _ = repo.CreateBranch(branchName, upstreamBranch.Tip);
            SetUpstream(repository, branchName, upstreamBranch.FriendlyName);

            branch = Commands.Checkout(repo, branchName);
        }

        return branch.FriendlyName == branchName;
    }

    public void Fetch(Repository repository)
    {
        var arguments = _appSettingsService.PruneOnFetch
            ? new string[]
                {
                    "fetch", "--all", "--prune",
                }
            : new string[] { "fetch", "--all", };

        _gitCommander.Command(repository, arguments);
    }

    public void Pull(Repository repository)
    {
        var arguments = _appSettingsService.PruneOnFetch
            ? new string[] { "pull", "--prune", }
            : new string[] { "pull", };

        _gitCommander.Command(repository, arguments);
    }

    public void Push(Repository repository)
    {
        _gitCommander.Command(repository, "push");
    }

    private void SetUpstream(Repository repository, string localBranchName, string upstreamBranchName)
    {
        _gitCommander.Command(repository, "branch", $"--set-upstream-to={upstreamBranchName}", localBranchName);
    }
}