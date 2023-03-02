namespace RepoM.Core.Plugin.RepositoryFiltering.Clause.Terms;

public class StartsWithTerm : TermBase
{
    public StartsWithTerm(string term, string value)
    {
        Term = term;
        Value = value;
    }

    public string Term { get; }

    public string Value { get; }

    public override string ToString()
    {
        return Term + ":" + Value + "*";
    }
}