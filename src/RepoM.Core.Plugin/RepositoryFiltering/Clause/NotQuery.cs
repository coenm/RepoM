namespace RepoM.Core.Plugin.RepositoryFiltering.Clause;

using System;

public class NotQuery : IQuery
{
    public NotQuery(IQuery item)
    {
        Item = item ?? throw new ArgumentNullException(nameof(item));
    }

    public IQuery Item { get; }

    public override string ToString()
    {
        return $"Not({Item})";
    }
}