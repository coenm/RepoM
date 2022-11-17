namespace RepoM.Api.IO.ModuleBasedRepositoryActionProvider.Data;

public class EvaluatedVariable
{
    public string? Name { get; set; }

    public object? Value { get; set; } = null!;
}