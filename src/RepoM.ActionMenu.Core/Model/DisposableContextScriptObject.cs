namespace RepoM.ActionMenu.Core.Model;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RepoM.ActionMenu.Core.ActionMenu.Context;
using RepoM.ActionMenu.Core.Yaml.Model.ActionContext;
using RepoM.ActionMenu.Interface.ActionMenuFactory;
using RepoM.ActionMenu.Interface.YamlModel;
using Scriban;
using Scriban.Parsing;
using Scriban.Runtime;

internal sealed class DisposableContextScriptObject : IScriptObject, IScope
{
    private readonly ActionMenuGenerationContext _context;
    private readonly IContextActionProcessor[] _mappers;
    private int _envCounter;
    private readonly IScriptObject _inner;

    internal DisposableContextScriptObject(ActionMenuGenerationContext context, IContextActionProcessor[] mappers) : this(context, mappers, new ScriptObject(), 0)
    {
    }

    private DisposableContextScriptObject(ActionMenuGenerationContext context, IContextActionProcessor[] mappers, IScriptObject so, int envCounter)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _mappers = mappers ?? throw new ArgumentNullException(nameof(mappers));
        _context.PushGlobal(this);
        _envCounter = envCounter;
        _inner = so;
    }
    
    public void Dispose()
    {
        if (_envCounter != 0)
        {
            while (_envCounter > 0)
            {
                _ = _context.Env.Pop();
                _envCounter--;
            }
        }

        _context.PopGlobal();
    }

    public Task AddContextActionAsync(IContextAction contextItem)
    {
        return AddContextActionInnerAsync(contextItem);
    }

    /// <summary>
    /// Clone DisposableContextScriptObject using a new context.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <returns>DisposableContextScriptObject</returns>
    public DisposableContextScriptObject CloneUsingNewContext(ActionMenuGenerationContext context)
    {
        return new DisposableContextScriptObject(
            context,
            _mappers,
            _inner.Clone(true),
            _envCounter);
    }

    private async Task AddContextActionInnerAsync(IContextAction contextItem)
    {
        var enabled = await IsActionEnabled(contextItem).ConfigureAwait(false);
        if (!enabled)
        {
            return;
        }

        IContextActionProcessor? mapper = Array.Find(_mappers, mapper => mapper.CanProcess(in contextItem));
        if (mapper == null)
        {
            throw new Exception("Cannot find mapper");
        }

        await mapper.ProcessAsync(contextItem, _context, this).ConfigureAwait(false);
    }

    private async Task<bool> IsActionEnabled(IContextAction contextItem)
    {
        // action does not implement interface and is therefore always enabled.
        if (contextItem is not IEnabled ea)
        {
            return true;
        }

        return await ea.Enabled.EvaluateAsync(_context).ConfigureAwait(false);
    }

    #region IScope
    void IScope.SetValue(string member, object? value, bool @readonly)
    {
        _inner.SetValue(member, value, @readonly);
    }

    void IScope.PushEnvironmentVariable(IDictionary<string, string> envVars)
    {
        _context.Env.Push(new EnvScriptObject(envVars));
        _envCounter++;
    }

    Task IScope.AddContextActionAsync(IContextAction contextItem)
    {
        return AddContextActionInnerAsync(contextItem);
    }
    #endregion

    #region IScriptObject
    int IScriptObject.Count => _inner.Count;

    bool IScriptObject.IsReadOnly
    {
        get => _inner.IsReadOnly;
        set => _inner.IsReadOnly = value;
    }

    IScriptObject IScriptObject.Clone(bool deep)
    {
        return _inner.Clone(deep);
    }

    IEnumerable<string> IScriptObject.GetMembers()
    {
        return _inner.GetMembers();
    }

    bool IScriptObject.Contains(string member)
    {
        return _inner.Contains(member);
    }

    bool IScriptObject.TryGetValue(TemplateContext context, SourceSpan span, string member, out object value)
    {
        return _inner.TryGetValue(context, span, member, out value);
    }

    bool IScriptObject.CanWrite(string member)
    {
        return _inner.CanWrite(member);
    }

    bool IScriptObject.TrySetValue(TemplateContext context, SourceSpan span, string member, object value, bool readOnly)
    {
        return _inner.TrySetValue(context, span, member, value, readOnly);
    }

    bool IScriptObject.Remove(string member)
    {
        return _inner.Remove(member);
    }

    void IScriptObject.SetReadOnly(string member, bool readOnly)
    {
        _inner.SetReadOnly(member, readOnly);
    }
    #endregion
}