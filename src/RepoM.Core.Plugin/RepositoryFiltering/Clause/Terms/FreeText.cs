namespace RepoM.Core.Plugin.RepositoryFiltering.Clause.Terms;

public class FreeText : TermBase
{
    public FreeText(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public override string ToString()
    {
        return Value;
    }
}