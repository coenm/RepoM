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
        if (text.StartsWith("is:pinned"))
        {
            isPinnedQuery = new SimpleTerm("is", "pinned");
            text = text.Replace("is:pinned", string.Empty);
        }
        else if(text.StartsWith("is:unpinned"))
        {
            isPinnedQuery = new SimpleTerm("is", "unpinned");
            text = text.Replace("is:unpinned", string.Empty);
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