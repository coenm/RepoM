namespace RepoM.Api.Common.IO.ModuleBasedRepositoryActionProvider.Data;

using ExpressionStringEvaluator.Methods;

public class Variable
{
    public string? Name { get; set; }

    public string? Value { get; set; }

    public string? Enabled { get; set; }
}

public class EvaluatedVariable
{
    public string? Name { get; set; }

    public CombinedTypeContainer Value { get; set; } = null!;

    public bool Enabled { get; set; } = true;
}