namespace RepoM.ActionMenu.Core.Abstractions;

using System.Collections.Generic;

/// <summary>
/// Abstraction of the environment.
/// </summary>
internal interface IEnvironment
{
    Dictionary<string, string> GetEnvironmentVariables();
}