namespace RepoM.Api.IO.ModuleBasedRepositoryActionProvider.Data;

using ExpressionStringEvaluator.Methods;

public class EvaluatedVariable
{
    public string? Name { get; set; }

    public CombinedTypeContainer Value { get; set; } = null!;
}