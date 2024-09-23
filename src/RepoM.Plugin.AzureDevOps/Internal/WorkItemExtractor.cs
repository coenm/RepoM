namespace RepoM.Plugin.AzureDevOps.Internal;

using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

internal static partial class WorkItemExtractor
{
    private static readonly Regex _workItemRegex = DevOpsTaskMatchingRegex();

    public static string[] GetDistinctWorkItemsFromCommitMessages(IEnumerable<string> commitMessages)
    {
        List<string> results = [];

        foreach (var commitMessage in commitMessages)
        {
            MatchCollection matches = _workItemRegex.Matches(commitMessage);
            if (matches.Any(m => m.Success))
            {
                results.AddRange(matches.SelectMany(m => m.Groups.Values.Skip(1)).Select(group => group.Value));
            }
        }

        return results.Distinct().ToArray();
    }

    [GeneratedRegex("\\#(\\d+)", RegexOptions.Compiled)]
    private static partial Regex DevOpsTaskMatchingRegex();
}