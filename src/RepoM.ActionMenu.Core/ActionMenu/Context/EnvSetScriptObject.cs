namespace RepoM.ActionMenu.Core.ActionMenu.Context;

using System;
using System.Collections.Generic;
using System.Linq;
using RepoM.ActionMenu.Core.Misc;
using Scriban;
using Scriban.Parsing;
using Scriban.Runtime;

internal sealed class EnvSetScriptObject : IScriptObject, IDisposable
{
    private FastStack<EnvScriptObject> _stack = new(10);

    public EnvSetScriptObject(IDictionary<string, string> envVars) : this(new EnvScriptObject(envVars))
    {
    }

    public EnvSetScriptObject(EnvScriptObject @base)
    {
        _ = @base ?? throw new ArgumentNullException(nameof(@base));
        _stack.Push(@base);
    }

    public void Push(EnvScriptObject item)
    {
        _stack.Push(item);
    }

    public EnvScriptObject Pop()
    {
        return _stack.Pop();
    }

    public IEnumerable<string> GetMembers()
    {
        return _stack.Items
            .Where(envScriptObject => envScriptObject != null)
            .SelectMany(envScriptObject => envScriptObject.GetMembers())
            .Distinct();
    }

    public bool Contains(string member)
    {
        return GetMembers().Contains(member);
    }

    public bool TryGetValue(TemplateContext context, SourceSpan span, string member, out object value)
    {
        EnvScriptObject? result = GetFirstForMember(member);

        if (result == null)
        {
            value = null!;
            return false;
        }

        return result.TryGetValue(context, span, member, out value);
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

    public EnvSetScriptObject Clone()
    {
        EnvScriptObject[] items = _stack.Items;
        var result = new EnvSetScriptObject(items[0].Clone());

        if (items.Length <= 1)
        {
            return result;
        }

        for (var i = 1; i < items.Length; i++)
        {
            if (items[i] != null)
            {
                result.Push(items[i].Clone());
            }
        }

        return result;
    }

    public int Count => GetMembers().Count();

    public bool IsReadOnly
    {
        get => true;
        set => _ = value;
    }

    public void Dispose()
    {
        _stack.Clear();
    }

    private EnvScriptObject? GetFirstForMember(string member)
    {
        var count = _stack.Count;
        EnvScriptObject[] items = _stack.Items;
        for (var i = count - 1; i >= 0; i--)
        {
            if (items[i].Contains(member))
            {
                return items[i];
            }
        }

        return null;
    }

    IScriptObject IScriptObject.Clone(bool deep)
    {
        return Clone();
    }
}