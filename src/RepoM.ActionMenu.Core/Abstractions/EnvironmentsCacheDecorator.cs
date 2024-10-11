namespace RepoM.ActionMenu.Core.Abstractions;

using System;
using System.Collections.Generic;

internal sealed class EnvironmentsCacheDecorator: IEnvironment
{
    private readonly IEnvironment _decoratee;
    private Dictionary<string, string>? _cachedValue;

    public EnvironmentsCacheDecorator(IEnvironment decoratee)
    {
        _decoratee = decoratee ?? throw new ArgumentNullException(nameof(decoratee));
    }

    public Dictionary<string, string> GetEnvironmentVariables()
    {
        return _cachedValue ??= _decoratee.GetEnvironmentVariables();
    }
}