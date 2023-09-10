namespace RepoM.Api.Git;

using System.Text;

public static class StatusCompressor
{
    private const int COMMIT_SHA_DISPLAY_CHARS = 7;

    public static string Compress(Repository repository)
    {
        if (string.IsNullOrEmpty(repository.CurrentBranch))
        {
            return string.Empty;
        }

        var printAddStagedRemoved = (repository.LocalAdded ?? 0) + (repository.LocalStaged ?? 0) + (repository.LocalRemoved ?? 0) > 0;
        var printUntrackedModifiedMissing = (repository.LocalUntracked ?? 0) + (repository.LocalModified ?? 0) + (repository.LocalMissing ?? 0) > 0;
        var printStashCount = (repository.StashCount ?? 0) > 0;

        var builder = new StringBuilder();

        var isAhead = (repository.AheadBy ?? 0) > 0;
        var isBehind = (repository.BehindBy ?? 0) > 0;
        var isOnCommitLevel = !isAhead && !isBehind;

        if (repository.CurrentBranchHasUpstream)
        {
            if (isOnCommitLevel)
            {
                builder.Append(StatusCharacterMap.IDENTICAL_SIGN);
            }
            else
            {
                if (isBehind)
                {
                    builder.Append($"{StatusCharacterMap.ARROW_DOWN_SIGN}{repository.BehindBy}");
                }

                if (isAhead)
                {
                    if (isBehind)
                    {
                        builder.Append(' ');
                    }

                    builder.Append($"{StatusCharacterMap.ARROW_UP_SIGN}{repository.AheadBy}");
                }
            }
        }
        else
        {
            builder.Append(StatusCharacterMap.NO_UPSTREAM_SIGN);
        }

        if (printAddStagedRemoved)
        {
            if (builder.Length > 0)
            {
                builder.Append(' ');
            }

            builder.Append($"+{repository.LocalAdded ?? 0} ~{repository.LocalStaged ?? 0} -{repository.LocalRemoved ?? 0}");
        }

        if (printUntrackedModifiedMissing)
        {
            if (builder.Length > 0)
            {
                builder.Append(' ');
            }

            if (printAddStagedRemoved)
            {
                builder.Append("| ");
            }

            builder.Append($"+{repository.LocalUntracked ?? 0} ~{repository.LocalModified ?? 0} -{repository.LocalMissing ?? 0}");
        }

        if (printStashCount)
        {
            if (builder.Length > 0)
            {
                builder.Append(' ');
            }

            builder.Append(StatusCharacterMap.STASH_SIGN);
            builder.Append(repository.StashCount);
        }

        return builder.ToString();
    }

    public static string CompressWithBranch(Repository repository)
    {
        var branch = repository.CurrentBranch;

        if (repository.CurrentBranchIsOnTag)
        {
            // put tabs in parenthesis ()
            branch = $"({branch})";
        }
        else
        {
            // put commit shas in parenthesis (), shorten them and show ellipses afterwards
            if (repository.CurrentBranchIsDetached && branch.Length > COMMIT_SHA_DISPLAY_CHARS)
            {
                branch = $"({branch[..COMMIT_SHA_DISPLAY_CHARS]}{StatusCharacterMap.ELLIPSES_SIGN})";
            }
        }

        return branch + " " + Compress(repository);
    }
}