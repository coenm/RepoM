namespace RepoM.Plugin.Heidi.Tests;

using System;
using System.Collections.Concurrent;
using System.Threading;


internal static class EnvironmentVariableManager
{
    private static readonly ConcurrentDictionary<string, AutoResetEvent> _envVarLocks = new();

    public static IDisposable SetEnvironmentVariable(string key, string value)
    {
        AutoResetEvent are = _envVarLocks.GetOrAdd(key, _ => new AutoResetEvent(true));
        are.WaitOne();

        var origValue = Environment.GetEnvironmentVariable(key);
        Environment.SetEnvironmentVariable(key, value);
        return new ReleaseDisposable(are, key, origValue);
    }

    private sealed class ReleaseDisposable : IDisposable
    {
        private readonly object _lock = new();
        private AutoResetEvent? _are;
        private readonly string _key;
        private readonly string? _value;

        public ReleaseDisposable(AutoResetEvent are, string key, string? value)
        {
            _are = are;
            _key = key;
            _value = value;
        }

        public void Dispose()
        {
            lock (_lock)
            {
                Environment.SetEnvironmentVariable(_key, _value);
                _are?.Set();
                _are = null;
            }
        }
    }
}