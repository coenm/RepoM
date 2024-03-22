namespace RepoM.ActionMenu.Core.Model;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RepoM.ActionMenu.Core.ActionMenu.Context;
using RepoM.ActionMenu.Core.Yaml.Model.ActionContext;
using RepoM.ActionMenu.Interface.ActionMenuFactory;
using RepoM.ActionMenu.Interface.YamlModel;
using Scriban.Runtime;

internal sealed class DisposableContextScriptObject : ScriptObject, IScope
{
    private readonly ActionMenuGenerationContext _context;
    private readonly EnvSetScriptObject _envSetScriptObject;
    private readonly List<IContextActionProcessor> _mappers;
    private int _envCounter;

    internal DisposableContextScriptObject(ActionMenuGenerationContext context, EnvSetScriptObject envSetScriptObject, List<IContextActionProcessor> mappers)
    {
        _envCounter = 0;
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _envSetScriptObject = envSetScriptObject ?? throw new ArgumentNullException(nameof(envSetScriptObject));
        _mappers = mappers ?? throw new ArgumentNullException(nameof(mappers));
        _context.PushGlobal(this);
    }

    public async Task AddContextActionAsync(IContextAction contextItem)
    {
        var enabled = await IsActionEnabled(contextItem).ConfigureAwait(false);
        if (!enabled)
        {
            return;
        }

        IContextActionProcessor? mapper = _mappers.Find(mapper => mapper.CanProcess(contextItem));
        if (mapper == null)
        {
            throw new Exception("Cannot find mapper");
        }

        await mapper.ProcessAsync(contextItem, _context, this).ConfigureAwait(false);
    }
    
    public void Dispose()
    {
        if (_envCounter != 0)
        {
            while (_envCounter > 0)
            {
                _ = _envSetScriptObject.Pop();
                _envCounter--;
            }
        }

        _context.PopGlobal();
    }

    public void PushEnvironmentVariable(IDictionary<string, string> envVars)
    {
        _envSetScriptObject.Push(new EnvScriptObject(envVars));
        _envCounter++;
    }

    private async Task<bool> IsActionEnabled(IContextAction contextItem)
    {
        // action does not implement interface and is therfore always enabled.
        if (contextItem is not IEnabled ea)
        {
            return true;
        }

        return await ea.Enabled.EvaluateAsync(_context).ConfigureAwait(false);
    }
}