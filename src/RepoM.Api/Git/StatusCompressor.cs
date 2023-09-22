namespace RepoM.Api.Git;

using System.Runtime.CompilerServices;
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
        
        if (repository.CurrentBranchHasUpstream)
        {
            AppendUpStream(repository, builder);
        }
        else
        {
            builder.Append(StatusCharacterMap.NO_UPSTREAM_SIGN);
        }

        if (printAddStagedRemoved)
        {
            AppendSpace(builder);
            builder.Append($"+{repository.LocalAdded ?? 0} ~{repository.LocalStaged ?? 0} -{repository.LocalRemoved ?? 0}");
        }

        if (printUntrackedModifiedMissing)
        {
            AppendSpace(builder);

            if (printAddStagedRemoved)
            {
                builder.Append("| ");
            }

            builder.Append($"+{repository.LocalUntracked ?? 0} ~{repository.LocalModified ?? 0} -{repository.LocalMissing ?? 0}");
        }

        if (printStashCount)
        {
            AppendSpace(builder);
            builder.Append(StatusCharacterMap.STASH_SIGN);
            builder.Append(repository.StashCount);
        }

        return builder.ToString().Trim();
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void AppendSpace(StringBuilder builder)
    {
        if (builder.Length > 0)
        {
            builder.Append(' ');
        }
    }

    private static void AppendUpStream(Repository repository, StringBuilder builder)
    {
        var isAhead = (repository.AheadBy ?? 0) > 0;
        var isBehind = (repository.BehindBy ?? 0) > 0;
        var isOnCommitLevel = !isAhead && !isBehind;

        if (isOnCommitLevel)
        {
            builder.Append(StatusCharacterMap.IDENTICAL_SIGN);
            return;
        }

        if (isBehind)
        {
            builder.Append($"{StatusCharacterMap.ARROW_DOWN_SIGN}{repository.BehindBy}");
        }

        if (!isAhead)
        {
            return;
        }

        if (isBehind)
        {
            builder.Append(' ');
        }

        builder.Append($"{StatusCharacterMap.ARROW_UP_SIGN}{repository.AheadBy}");
    }
}