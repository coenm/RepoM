namespace RepoM.Api.IO.Variables;

using System;
using System.Collections.Generic;
using System.Threading;
using RepoM.Core.Plugin.Repository;

public static class EnvironmentVariableStore
{
    private static readonly ThreadLocal<Dictionary<string, string>> _envVars = new();
    
    public static IDisposable Set(Dictionary<string, string>? envVars)
    {
        if (envVars == null)
        {
            return new ExecuteOnDisposed(null);
        }

        _envVars.Value = envVars;
        return new ExecuteOnDisposed(() => _envVars.Value = new Dictionary<string, string>());
    }

    public static Dictionary<string, string> Get(IRepository _)
    {
        return _envVars.Value ?? new Dictionary<string, string>(0);
    }

    private sealed class ExecuteOnDisposed : IDisposable
    {
        private readonly Func<Dictionary<string, string>>? _func;

        public ExecuteOnDisposed(Func<Dictionary<string, string>>? func)
        {
            _func = func;
        }

        public void Dispose()
        {
            _func?.Invoke();
        }
    }
}