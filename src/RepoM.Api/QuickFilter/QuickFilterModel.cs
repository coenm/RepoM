namespace RepoM.Api.QuickFilter;

using System;
using Newtonsoft.Json;
using RepoM.Core.Plugin.RepositoryFiltering.Clause;

public sealed class QuickFilterModel
{
    public Guid Id { get; set; }

    public string Label { get; set; } = string.Empty;

    public IQuery Query { get; set; } = TrueQuery.Instance;

    public bool IsActive { get; set; }

    public bool IsInverse { get; set; }

    public int Order { get; set; }

    public string ToolTip { get; set; } = string.Empty;

    [JsonIgnore]
    public bool IsBuiltIn { get; set; }
}