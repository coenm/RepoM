namespace RepoM.Core.Plugin.Common;

using System;

public interface IClock
{
    DateTime Now { get; }
}