namespace RepoM.UI.Services;

/// <summary>Update information presented by the UI, independent of its host.</summary>
public interface IApplicationUpdate
{
    string? AvailableUpdate { get; }
}

public sealed class ApplicationUpdate : IApplicationUpdate
{
    public string? AvailableUpdate { get; set; }
}
