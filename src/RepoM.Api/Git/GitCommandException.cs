namespace RepoM.Api.Git;

using System;
using System.Diagnostics;

public class GitCommandException : Exception
{
    public GitCommandException(string message, Process process)
        : base(message)
    {
        Process = process;
    }

    public Process? Process { get; private set; }
}