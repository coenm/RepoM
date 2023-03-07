namespace RepoM.Api.Tests.Git;

using System;
using System.Collections.Generic;
using RepoM.Api.Git;

internal class RepositoryBuilder
{
    private readonly List<Action<Repository>> _actions = new();
    private string _path = string.Empty;

    public RepositoryBuilder WithName(string name)
    {
        _actions.Add(r => r.Name = name);
        return this;
    }

    public RepositoryBuilder WithPath(string path)
    {
        _path = path;
        return this;
    }

    public RepositoryBuilder WithAheadBy(int ahead)
    {
        _actions.Add(r => r.AheadBy = ahead);
        return this;
    }

    public RepositoryBuilder WithBehindBy(int behind)
    {
        _actions.Add(r => r.BehindBy = behind);
        return this;
    }

    public RepositoryBuilder WithBranches(params string[] branches)
    {
        _actions.Add(r => r.Branches = branches);
        return this;
    }

    public RepositoryBuilder WithCurrentBranch(string currentBranch)
    {
        _actions.Add(r => r.CurrentBranch = currentBranch);
        return this;
    }

    public RepositoryBuilder WithDetachedHeadOnCommit(string sha)
    {
        _actions.Add(r =>
            {
                r.CurrentBranchIsDetached = true;
                r.CurrentBranch = sha;
            });
        return this;
    }

    public RepositoryBuilder WithDetachedHeadOnTag(string tag)
    {
        _actions.Add(r =>
            {
                r.CurrentBranchIsDetached = true;
                r.CurrentBranchIsOnTag = true;
                r.CurrentBranch = tag;
            });
        return this;
    }

    public RepositoryBuilder WithUpstream()
    {
        _actions.Add(r => r.CurrentBranchHasUpstream = true);
        return this;
    }

    public RepositoryBuilder WithoutUpstream()
    {
        _actions.Add(r => r.CurrentBranchHasUpstream = false);
        return this;
    }

    public RepositoryBuilder WithLocalAdded(int added)
    {
        _actions.Add(r => r.LocalAdded = added);
        return this;
    }

    public RepositoryBuilder WithLocalIgnored(int ignored)
    {
        _actions.Add(r => r.LocalIgnored = ignored);
        return this;
    }

    public RepositoryBuilder WithLocalMissing(int missing)
    {
        _actions.Add(r => r.LocalMissing = missing);
        return this;
    }

    public RepositoryBuilder WithLocalModified(int modified)
    {
        _actions.Add(r => r.LocalModified = modified);
        return this;
    }

    public RepositoryBuilder WithLocalRemoved(int removed)
    {
        _actions.Add(r => r.LocalRemoved = removed);
        return this;
    }

    public RepositoryBuilder WithLocalStaged(int staged)
    {
        _actions.Add(r => r.LocalStaged = staged);
        return this;
    }

    public RepositoryBuilder WithLocalUntracked(int untracked)
    {
        _actions.Add(r => r.LocalUntracked = untracked);
        return this;
    }

    public RepositoryBuilder WithStashCount(int stashCount)
    {
        _actions.Add(r => r.StashCount = stashCount);
        return this;
    }

    public Repository Build()
    {
        var repo = new Repository(_path);
        foreach (Action<Repository> action in _actions)
        {
            action.Invoke(repo);
        }
        return repo;
    }

    public RepositoryBuilder FullFeatured()
    {
        WithUpstream();
        WithAheadBy(1);
        WithBehindBy(2);
        WithBranches("master", "feature-one", "feature-two");
        WithCurrentBranch("master");
        WithLocalAdded(3);
        WithLocalIgnored(4);
        WithLocalMissing(5);
        WithLocalModified(6);
        WithLocalRemoved(7);
        WithLocalStaged(8);
        WithLocalUntracked(9);
        WithStashCount(10);
        WithName("Repo");
        WithPath("C:\\Develop\\Repo\\");
        return this;
    }

    public Repository BuildFullFeatured()
    {
        FullFeatured();
        return Build();
    }
}