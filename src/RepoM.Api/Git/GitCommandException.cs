namespace RepoM.Api.Git;

using System;
using System.Diagnostics;

public class GitCommandException : Exception
{
    public Process? Process { get; private set; }

    public GitCommandException(string message, Process process)
        : base(message)
    {
        Process = process;
    }

  protected GitCommandException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
        : base(info, context)
  {
      Process = null;
  }
}