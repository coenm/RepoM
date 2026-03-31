namespace RepoM.App.RepositoryFiltering;

using JetBrains.Annotations;
using RepoM.Core.Plugin.RepositoryFiltering;
using RepoM.Core.Plugin.RepositoryFiltering.Clause;
using RepoM.Core.Plugin.RepositoryFiltering.Clause.Terms;

[UsedImplicitly]
internal class DefaultQueryParser : INamedQueryParser
{
    public string Name => "Default";

    public IQuery Parse(string text)
    {
        IQuery? isPinnedQuery = null;
        if (text.StartsWith("is:pinned") || text.StartsWith("is:starred") || text.StartsWith("is:favorite"))
        {
            isPinnedQuery = new SimpleTerm("is", "pinned");
            text = text
                .Replace("is:pinned", string.Empty)
                .Replace("is:starred", string.Empty)
                .Replace("is:favorite", string.Empty);
        }
        else if(text.StartsWith("is:unpinned") || text.StartsWith("is:unstarred") || text.StartsWith("is:unfavorite"))
        {
            isPinnedQuery = new SimpleTerm("is", "unpinned");
            text = text
                .Replace("is:unpinned", string.Empty)
                .Replace("is:unstarred", string.Empty)
                .Replace("is:unfavorite", string.Empty);
        }

        if (isPinnedQuery != null && string.IsNullOrWhiteSpace(text))
        {
            return isPinnedQuery;
        }

        var freeTextQuery = new FreeText(text);

        return isPinnedQuery == null
            ? freeTextQuery
            : new AndQuery(isPinnedQuery, freeTextQuery);
    }
}