namespace RepoM.Api.Git.ProcessExecution;

using System;

[Serializable]
public class GitCommandException : Exception
{
    public GitCommandException() { } 

    public GitCommandException(string message) : base(message) { }

    public GitCommandException(string message, Exception innerException) : base(message, innerException) { }

    protected GitCommandException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
}