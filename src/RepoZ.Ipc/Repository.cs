namespace RepoZ.Ipc;

using System;

[System.Diagnostics.DebuggerDisplay("{Name}")]
public class Repository
{
    public Repository(string name)
    {
        Name = name;
    }

    public static Repository? FromString(string value)
    {
        var parts = value.Split(new string[] { "::", }, System.StringSplitOptions.None);
        var partsCount = parts.Length;

        var validFormat = partsCount is 3 or 4;
        if (!validFormat)
        {
            return null;
        }

        return new Repository(parts[0])
            {
                BranchWithStatus = parts[1],
                Path = parts[2],
                HasUnpushedChanges = parts.Length > 3 && parts[3] == "1",
            };
    }

    public override string ToString()
    {
        if (string.IsNullOrEmpty(Name) || string.IsNullOrEmpty(BranchWithStatus) || string.IsNullOrEmpty(Path))
        {
            return "";
        }

        return $"{Name}::{BranchWithStatus}::{Path}::{(HasUnpushedChanges ? "1" : "0")}";
    }

    public string Name { get; }

    public string? BranchWithStatus { get; set; }

    public string? Path { get; set; }

    public bool HasUnpushedChanges { get; set; }

    public string SafePath => Path?.Replace(@"\", "/") ?? string.Empty;

    public string[] ReadAllBranches()
    {
        return Array.Empty<string>();
    }
}