namespace RepoM.Core.Plugin.Common;

using System;

/// <summary>
/// An abstraction of System.DateTime.
/// </summary>
public interface IClock
{
    DateTime Now { get; }
}