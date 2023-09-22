namespace RepoM.Api.IO.ModuleBasedRepositoryActionProvider.Data;

using System.Diagnostics;

[DebuggerDisplay("{Name,nq}")]
public class EvaluatedVariable
{
    public string? Name { get; set; }

    public object? Value { get; set; } = null!;
}