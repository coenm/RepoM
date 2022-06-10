namespace RepoM.Plugin.LuceneSearch;

using System.Collections.Generic;

internal class RepositorySearchModel
{
    public RepositorySearchModel(string repositoryName, string path, List<string> tags)
    {
        RepositoryName = repositoryName;
        Path = path;
        Tags = tags;
    }

    public string RepositoryName { get; }

    public string Path { get; }

    public List<string> Tags { get; }
}