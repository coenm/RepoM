namespace RepoM.Plugin.LuceneQueryParser.Plugin.Clause.Terms;

using System.Net.Http.Headers;

public class SimpleTerm : TermBase
{
    public SimpleTerm(string term, string value)
    {
        Term = term;
        Value = value;
    }

    public string Term { get; }

    public string Value { get; }

    public override string ToString()
    {
        return Term + ":" + Value;
    }
}