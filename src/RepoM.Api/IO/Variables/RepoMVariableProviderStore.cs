namespace RepoM.Api.IO.Variables;

using System;
using System.Collections.Generic;
using System.Threading;
using RepoM.Api.IO.ModuleBasedRepositoryActionProvider.Data;

public static class RepoMVariableProviderStore
{
    public static readonly ThreadLocal<Scope?> VariableScope = new();

    public static IDisposable Push(List<EvaluatedVariable> vars)
    {
        VariableScope.Value = new Scope(VariableScope.Value, vars);
        return VariableScope.Value;
    }

    public static IDisposable Set(Scope scope)
    {
        VariableScope.Value = scope;
        return VariableScope.Value;
    }
}