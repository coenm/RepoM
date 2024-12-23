namespace RepoM.Api.Git;

using System;
using System.Linq;
using LibGit2Sharp;
using RepoM.Api.Common;
using IRepository = RepoM.Core.Plugin.Repository.IRepository;

public class DefaultRepositoryWriter : IRepositoryWriter
{
    private readonly IGitCommander _gitCommander;
    private readonly IAppSettingsService _appSettingsService;

    public DefaultRepositoryWriter(IGitCommander gitCommander, IAppSettingsService appSettingsService)
    {
        _gitCommander = gitCommander ?? throw new ArgumentNullException(nameof(gitCommander));
        _appSettingsService = appSettingsService ?? throw new ArgumentNullException(nameof(appSettingsService));
    }

    public bool Checkout(IRepository repository, string branchName)
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

    public void Fetch(IRepository repository)
    {
        var arguments = _appSettingsService.PruneOnFetch
            ? ["fetch", "--all", "--prune",]
            : new[] { "fetch", "--all", };

        _gitCommander.Command(repository, arguments);
    }

    public void Pull(IRepository repository)
    {
        var arguments = _appSettingsService.PruneOnFetch
            ? ["pull", "--prune",]
            : new[] { "pull", };

        _gitCommander.Command(repository, arguments);
    }

    public void Push(IRepository repository)
    {
        _gitCommander.Command(repository, "push");
    }

    private void SetUpstream(IRepository repository, string localBranchName, string upstreamBranchName)
    {
        _gitCommander.Command(repository, "branch", $"--set-upstream-to={upstreamBranchName}", localBranchName);
    }
}