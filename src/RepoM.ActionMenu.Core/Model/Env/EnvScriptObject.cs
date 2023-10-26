namespace RepoM.ActionMenu.Core.Model.Env;

using System;
using System.Collections;
using System.Collections.Generic;
using Scriban;
using Scriban.Parsing;
using Scriban.Runtime;

internal sealed class EnvScriptObject : IScriptObject
{
    private readonly Dictionary<string, string> _env;

    public static EnvScriptObject Create()
    {
        var env = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        foreach (DictionaryEntry item in Environment.GetEnvironmentVariables()) // difficult to test.
        {
            if (item.Key is not string key || string.IsNullOrEmpty(key))
            {
                continue;
            }

            if (item.Value is not string value)
            {
                continue;
            }

            env.Add(key.Trim(), value);
        }

        return new EnvScriptObject(env);
    }

    public EnvScriptObject(Dictionary<string, string> envVars)
    {
        _env = envVars;
    }

    public IEnumerable<string> GetMembers()
    {
        return _env.Keys;
    }

    public bool Contains(string member)
    {
        return _env.ContainsKey(member);
    }

    public bool TryGetValue(TemplateContext context, SourceSpan span, string member, out object value)
    {
        if (_env.TryGetValue(member, out string? s))
        {
            value = s;
            return true;
        }

        value = null!;
        return false;
    }

    public bool CanWrite(string member)
    {
        return false;
    }

    public bool TrySetValue(TemplateContext context, SourceSpan span, string member, object value, bool readOnly)
    {
        return false;
    }

    public bool Remove(string member)
    {
        return false;
    }

    public void SetReadOnly(string member, bool readOnly)
    {
        // intentionally do nothing
    }

    public IScriptObject Clone(bool deep)
    {
        return new EnvScriptObject(new Dictionary<string, string>(_env));
    }

    public int Count => _env.Count;

    public bool IsReadOnly
    {
        get => true;
        set => _ = value;
    }
}