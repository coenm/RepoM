namespace RepoM.ActionMenu.Core.Abstractions;

using System.Collections.Generic;

internal interface IEnvironment
{
    Dictionary<string, string> GetEnvironmentVariables();
}