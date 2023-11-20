namespace RepoM.ActionMenu.Core.Model;

using System;
using RepoM.ActionMenu.Interface.Scriban;
using Scriban.Runtime;

internal class RepoMScriptObject : ScriptObject, IContextRegistration
{
    public override IScriptObject Clone(bool deep)
    {
        // todo not sure if this clone is okay.
        var result = base.Clone(deep);

        if (result is RepoMScriptObject r)
        {
            return r;
        }

        throw new NotImplementedException("Could not clone");
    }

    IContextRegistration IContextRegistration.CreateOrGetSubRegistration(string key)
    {
        if (this.TryGetValue(key, out object value))
        {
            if (value is IContextRegistration cr)
            {
                return cr;
            }

            throw new Exception($"Object registered as '{key}' is does not implement '{nameof(IContextRegistration)}'");
        }

        var sub = new RepoMScriptObject();
        SetValue(key, sub, false);
        return sub;
    }

    void IContextRegistration.SetValue(string member, object value, bool readOnly)
    {
        SetValue(member, value, readOnly);
    }

    void IContextRegistration.Add(string key, object value)
    {
        Add(key, value);
    }

    bool IContextRegistration.ContainsKey(string key)
    {
        return ContainsKey(key);
    }
    
    void IContextRegistration.RegisterConstant(string name, object value)
    {
        RegisterVariableInner(name, value);
    }

    void IContextRegistration.RegisterAction(string name, Action action)
    {
        RegisterCustomFunction(name, DelegateCustomFunction.Create(action));
    }

    void IContextRegistration.RegisterAction<T1>(string name, Action<T1> action)
    {
        RegisterCustomFunction(name, DelegateCustomFunction.Create(action));
    }

    void IContextRegistration.RegisterAction<T1, T2>(string name, Action<T1, T2> action)
    {
        RegisterCustomFunction(name, DelegateCustomFunction.Create(action));
    }

    void IContextRegistration.RegisterAction<T1, T2, T3>(string name, Action<T1, T2, T3> action)
    {
        RegisterCustomFunction(name, DelegateCustomFunction.Create(action));
    }

    void IContextRegistration.RegisterAction<T1, T2, T3, T4>(string name, Action<T1, T2, T3, T4> action)
    {
        RegisterCustomFunction(name, DelegateCustomFunction.Create(action));
    }

    void IContextRegistration.RegisterAction<T1, T2, T3, T4, T5>(string name, Action<T1, T2, T3, T4, T5> action)
    {
        RegisterCustomFunction(name, DelegateCustomFunction.Create(action));
    }

    void IContextRegistration.RegisterFunction<T1>(string name, Func<T1> func)
    {
        RegisterCustomFunction(name, DelegateCustomFunction.CreateFunc(func));
    }

    void IContextRegistration.RegisterFunction<T1, T2>(string name, Func<T1, T2> func)
    {
        RegisterCustomFunction(name, DelegateCustomFunction.CreateFunc(func));
    }

    void IContextRegistration.RegisterFunction<T1, T2, T3>(string name, Func<T1, T2, T3> func)
    {
        RegisterCustomFunction(name, DelegateCustomFunction.CreateFunc(func));
    }

    void IContextRegistration.RegisterFunction<T1, T2, T3, T4>(string name, Func<T1, T2, T3, T4> func)
    {
        RegisterCustomFunction(name, DelegateCustomFunction.CreateFunc(func));
    }

    void IContextRegistration.RegisterFunction<T1, T2, T3, T4, T5>(string name, Func<T1, T2, T3, T4, T5> func)
    {
        RegisterCustomFunction(name, DelegateCustomFunction.CreateFunc(func));
    }

    void IContextRegistration.RegisterFunction<T1, T2, T3, T4, T5, T6>(string name, Func<T1, T2, T3, T4, T5, T6> func)
    {
        RegisterCustomFunction(name, DelegateCustomFunction.CreateFunc(func));
    }

    void IContextRegistration.RegisterVariable(string name, object value)
    {
        RegisterVariableInner(name, value);
    }

    private void RegisterCustomFunction(string name, IScriptCustomFunction func)
    {
        RegisterVariableInner(name, func);
    }

    private void RegisterVariableInner(string name, object value)
    {
        if (name == null)
        {
            throw new ArgumentNullException(nameof(name));
        }

        if (value == null)
        {
            throw new ArgumentNullException(nameof(value));
        }

        var names = name.Split(',');


        foreach (var subName in names)
        {
            SetValue(subName, value, true);
        }
    }
}