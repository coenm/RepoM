namespace RepoM.Plugin.LuceneSearch.Tests.NewFolder.Clause.Terms;
public class Or : TermBase
{
    public Or(params TermBase[] terms)
    {
        Terms = terms;
    }

    public TermBase[] Terms { get; }
}