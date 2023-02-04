namespace RepoM.Plugin.LuceneSearch.Tests.NewFolder.Clause.Terms;
public class SimpleTerm : TermBase
{
    public SimpleTerm(string term, string value)
    {
        Term = term;
        Value = value;
    }

    public string Term { get; }

    public string Value { get; }
}