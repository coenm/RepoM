namespace RepoM.Core.Repositories.Scanning;

using System;

public record GitRepositoryScannerSettings
{
    public GitRepositoryScannerSettings(int degreeOfParallelism)
    {
        DegreeOfParallelism = degreeOfParallelism >= 1
            ? degreeOfParallelism
            : throw new ArgumentOutOfRangeException(nameof(degreeOfParallelism), degreeOfParallelism, "Degree of parallelism must be at least 1.");
    }

    public int DegreeOfParallelism { get; }
}
