namespace RepoM.App.RepositoryFiltering;

using System.Collections.Generic;
using System.Linq;
using RepoM.Core.Plugin.RepositoryFiltering;
using RepoM.Core.Plugin.RepositoryFiltering.Clause;
using RepoM.Core.Plugin.RepositoryFiltering.Clause.Terms;

internal class DefaultQueryParser : INamedQueryParser
{
    public string Name => "Default";

    public IQuery Parse(string text)
    {
        List<IQuery> q = new();
        if (text.StartsWith("is:pinned"))
        {
            q.Add(new SimpleTerm("is", "pinned"));
            text = text.Replace("is:pinned", " ");
        }
        else if(text.StartsWith("is:unpinned"))
        {
            q.Add(new SimpleTerm("is", "unpinned"));
            text = text.Replace("is:unpinned", " ");
        }

        q.Add(new FreeText(text));

        if (q.Any())
        {
            return new AndQuery(q.ToArray());

        }

        return q.First();
    }
}