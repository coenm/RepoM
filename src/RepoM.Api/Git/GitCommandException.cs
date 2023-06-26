namespace RepoM.Api.Git;

using System;
using System.Diagnostics;
using System.Runtime.Serialization;

[Serializable]
public class GitCommandException : Exception
{
    public Process? Process { get; private set; }

    public GitCommandException(string message, Process process)
        : base(message)
    {
        Process = process;
    }

    protected GitCommandException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
        Process = null;
    }

    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        base.GetObjectData(info, context);
        // process cannot be written to serialization info.
    }
}