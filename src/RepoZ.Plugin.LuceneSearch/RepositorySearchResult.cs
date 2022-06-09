namespace RepoZ.Plugin.LuceneSearch;

using System.Collections.Generic;

internal class RepositorySearchResult : RepositorySearchModel
{
    internal RepositorySearchResult(string repositoryName, string path, List<string> tags, float score) :
        base(repositoryName, path, tags)
    {
        Score = score;
    }

    public float Score { get; }
}