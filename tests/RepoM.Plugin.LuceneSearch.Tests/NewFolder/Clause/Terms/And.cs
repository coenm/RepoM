namespace RepoM.Plugin.LuceneSearch.Tests.NewFolder.Clause.Terms;
public class And : TermBase
{
    public And(params TermBase[] terms)
    {
        Terms = terms;
    }

    public TermBase[] Terms { get; }
}