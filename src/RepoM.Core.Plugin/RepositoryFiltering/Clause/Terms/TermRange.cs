namespace RepoM.Core.Plugin.RepositoryFiltering.Clause.Terms;

public class TermRange : TermBase
{
    public TermRange(string field, string? lowerText, bool includesLower, string? upperText, bool includesUpper)
    {
        Field = field;
        LowerText = lowerText;
        IncludesLower = includesLower;
        UpperText = upperText;
        IncludesUpper = includesUpper;
    }

    public string Field { get; }

    public string? LowerText { get; }

    public bool IncludesLower { get; }

    public string? UpperText { get; }

    public bool IncludesUpper { get; }

    public override string ToString()
    {
        return Field + ":[" + (LowerText ?? "*") + ".." + (UpperText ?? "*") + "]";
    }
}