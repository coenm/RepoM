namespace RepoM.Plugin.AzureDevOps.Internal;

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using RepoM.Core.Plugin.Repository;

internal static class CommitMessageExtractor
{
    public static IEnumerable<string> GetCommitMessagesUntilBranch(IRepository repository, string toBranch, ILogger logger)
    {
        try
        {
            using var repo = new LibGit2Sharp.Repository(repository.Path);

            var commitMessages = repo.Commits
                                     .QueryBy(new LibGit2Sharp.CommitFilter
                                        {
                                             ExcludeReachableFrom = repo.Branches[toBranch].UpstreamBranchCanonicalName,
                                             FirstParentOnly = true,
                                        })
                                     .Select(c => c.Message)
                                     .ToArray();
            return commitMessages;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to get commit messages until branch {Branch}", toBranch);
            return Array.Empty<string>();
        }
    }
}