namespace RepoM.Plugin.AzureDevOps.Internal;

using System;
using System.Collections.Generic;
using System.Linq;
using RepoM.Core.Plugin.Repository;

internal static class CommitMessageExtractor
{
    public static IEnumerable<string> GetCommitMessagesUntilBranch(IRepository repository, string toBranch)
    {
        try
        {
            using var repo = new LibGit2Sharp.Repository(repository.Path);

            var commitMessages = repo.Commits
                                     .QueryBy(new LibGit2Sharp.CommitFilter
                                         {
                                             ExcludeReachableFrom = repo.Branches[toBranch].UpstreamBranchCanonicalName,
                                         })
                                     .Select(c => c.Message)
                                     .ToArray();
            return commitMessages;
        }
        catch (Exception)
        {
            // swallow for now, when moved to IRepository, use logger.
            return Array.Empty<string>();
        }
    }
}