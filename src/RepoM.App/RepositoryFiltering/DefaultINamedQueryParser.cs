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
        IQuery? favoriteQuery = null;
        if (text.StartsWith("is:pinned") || text.StartsWith("is:starred") || text.StartsWith("is:favorite"))
        {
            favoriteQuery = new SimpleTerm("is", "favorite");
            text = text
                .Replace("is:pinned", string.Empty)
                .Replace("is:starred", string.Empty)
                .Replace("is:favorite", string.Empty);
        }
        else if(text.StartsWith("is:unpinned") || text.StartsWith("is:unstarred") || text.StartsWith("is:unfavorite"))
        {
            favoriteQuery = new SimpleTerm("is", "unfavorite");
            text = text
                .Replace("is:unpinned", string.Empty)
                .Replace("is:unstarred", string.Empty)
                .Replace("is:unfavorite", string.Empty);
        }

        if (favoriteQuery != null && string.IsNullOrWhiteSpace(text))
        {
            return favoriteQuery;
        }

        var freeTextQuery = new FreeText(text);

        return favoriteQuery == null
            ? freeTextQuery
            : new AndQuery(favoriteQuery, freeTextQuery);
    }
}