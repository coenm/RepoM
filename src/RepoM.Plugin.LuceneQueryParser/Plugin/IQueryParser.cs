namespace RepoM.Plugin.LuceneQueryParser.Plugin;

using RepoM.Plugin.LuceneQueryParser.Plugin.Clause;

public interface IQueryParser
{
    IQuery Parse(string text);
}