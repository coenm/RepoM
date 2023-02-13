namespace RepoM.Core.Plugin.RepositoryFiltering;

using RepoM.Core.Plugin.RepositoryFiltering.Clause;

public interface IQueryParser
{
    IQuery Parse(string text);
}