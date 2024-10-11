namespace RepoM.ActionMenu.Core.ActionMenu.Context;

using System;
using System.Collections.Generic;
using Scriban;
using Scriban.Parsing;
using Scriban.Runtime;

internal sealed class EnvScriptObject : IScriptObject
{
    private readonly IDictionary<string, string> _env;

    public EnvScriptObject(IDictionary<string, string> envVars)
    {
        _env = envVars ?? throw new ArgumentNullException(nameof(envVars));
    }

    public int Count => _env.Count;

    public bool IsReadOnly
    {
        get => true;
        set => _ = value;
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
        if (_env.TryGetValue(member, out var s))
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

    public EnvScriptObject Clone()
    {
        return new EnvScriptObject(new Dictionary<string, string>(_env));
    }

    IScriptObject IScriptObject.Clone(bool deep)
    {
        return Clone();
    }
}