namespace Grrui.Model;

using System;
using RepoM.Api.Git;
using Repository = RepoM.Ipc.Repository;

public class RepositoryView : IRepositoryView
{
    public RepositoryView(Repository repository, string displayText)
    {
        Repository = repository ?? throw new ArgumentNullException(nameof(repository));
        DisplayText = displayText;
    }

    public override string ToString()
    {
        return DisplayText;
    }

    public Repository Repository { get; }

    public string DisplayText { get; }

    public string Name => Repository?.Name ?? "";

    public string CurrentBranch => Repository?.BranchWithStatus ?? string.Empty;

    public string[] ReadAllBranches()
    {
        return Repository.ReadAllBranches() ?? Array.Empty<string>();
    }

    public string Path => Repository.Path ?? string.Empty;

    public bool HasUnpushedChanges => Repository.HasUnpushedChanges;
}