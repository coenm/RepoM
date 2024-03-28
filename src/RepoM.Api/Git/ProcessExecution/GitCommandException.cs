namespace RepoM.Api.Git.ProcessExecution;

using System;

public class GitCommandException : Exception
{
    public GitCommandException() { } 

    public GitCommandException(string message) : base(message) { }

    public GitCommandException(string message, Exception innerException) : base(message, innerException) { }
}