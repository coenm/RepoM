namespace RepoM.Plugin.Misc.Tests.TestFramework;

internal static class MarkdownHelpers
{
    public static string CreateGithubMarkdownAnchor(string input)
    {
        return input
               .Replace("@", string.Empty)
               .Replace(" ", "-")
               .ToLower();
    }
}