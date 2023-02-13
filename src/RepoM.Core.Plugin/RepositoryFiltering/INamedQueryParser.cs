namespace RepoM.Core.Plugin.RepositoryFiltering;

public interface INamedQueryParser : IQueryParser
{
    string Name { get; }
}