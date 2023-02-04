namespace RepoM.Plugin.LuceneSearch.Tests.NewFolder.Clause.Terms;
public class Not : TermBase
{
    public Not(TermBase term)
    {
        Term = term;
    }

    public TermBase Term { get; }
}

//