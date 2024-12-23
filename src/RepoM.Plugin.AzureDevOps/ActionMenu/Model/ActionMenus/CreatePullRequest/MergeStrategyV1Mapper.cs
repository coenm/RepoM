namespace RepoM.Plugin.AzureDevOps.ActionMenu.Model.ActionMenus.CreatePullRequest;

using System;
using RepoM.Plugin.AzureDevOps.RepositoryCommands;

internal static class MergeStrategyV1Mapper
{
    public static CreatePullRequestRepositoryCommand.MergeStrategy MapToDomain(this MergeStrategyV1 input)
    {
        return input switch
            {
                MergeStrategyV1.NoFastForward => CreatePullRequestRepositoryCommand.MergeStrategy.NoFastForward,
                MergeStrategyV1.Squash => CreatePullRequestRepositoryCommand.MergeStrategy.Squash,
                MergeStrategyV1.Rebase => CreatePullRequestRepositoryCommand.MergeStrategy.Rebase,
                MergeStrategyV1.RebaseMerge => CreatePullRequestRepositoryCommand.MergeStrategy.RebaseMerge,
                _ => throw new ArgumentOutOfRangeException(nameof(input), input, null),
            };
    }
}