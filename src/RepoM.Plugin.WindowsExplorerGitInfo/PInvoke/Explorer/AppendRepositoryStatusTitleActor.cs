namespace RepoM.Plugin.WindowsExplorerGitInfo.PInvoke.Explorer;

using System;
using System.Linq;
using RepoM.Api.Git;
using RepoM.Core.Repositories.Model;
using RepoM.Core.Repositories.Store;

internal class AppendRepositoryStatusTitleActor : ExplorerWindowActor
{
    private readonly IRepositoryStore _repositoryStore;

    public AppendRepositoryStatusTitleActor(IRepositoryStore repositoryStore)
    {
        _repositoryStore = repositoryStore;
    }

    protected override void Act(IntPtr hwnd, string? explorerLocationUrl)
    {
        if (string.IsNullOrEmpty(explorerLocationUrl))
        {
            return;
        }

        var path = new Uri(explorerLocationUrl).LocalPath;

        var status = GetStatusByPath(path);

        if (string.IsNullOrEmpty(status))
        {
            return;
        }

        const string SEPARATOR = "  [";
        WindowHelper.AppendWindowText(hwnd, SEPARATOR, status + "]");
    }

    private string? GetStatusByPath(string path)
    {
        if (string.IsNullOrEmpty(path))
        {
            return null;
        }

        if (!path.EndsWith('\\'))
        {
            path += "\\";
        }

        RepositoryInfo? match = _repositoryStore.Items
            .Where(r => r.Path != null && path.StartsWith(r.Path, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(r => r.Path.Length)
            .FirstOrDefault();

        if (match == null)
        {
            return null;
        }

        return StatusCompressor.CompressWithBranch(match);
    }
}
