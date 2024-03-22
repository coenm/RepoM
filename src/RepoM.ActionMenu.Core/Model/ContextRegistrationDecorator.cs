namespace RepoM.ActionMenu.Core.Model;

using System;
using RepoM.ActionMenu.Interface.Scriban;
using Scriban;
using Scriban.Runtime;


#pragma warning disable S2436
// Reduce the number of generic parameters in the 'InternalDelegateCustomFunctionWithInterfaceContext' class to no more than the 2 authorized.
// https://rules.sonarsource.com/csharp/RSPEC-2436/

internal class ContextRegistrationDecorator<T> : IContextRegistration where T : TemplateContext
{
    private readonly IContextRegistration _contextRegistration;

    public ContextRegistrationDecorator(IContextRegistration contextRegistration)
    {
        _contextRegistration = contextRegistration ?? throw new ArgumentNullException(nameof(contextRegistration));
    }

    public IContextRegistration CreateOrGetSubRegistration(string key)
    {
        return new ContextRegistrationDecorator<T>(_contextRegistration.CreateOrGetSubRegistration(key));
    }

    public void SetValue(string member, object value, bool readOnly)
    {
        _contextRegistration.SetValue(member, value, readOnly);
    }

    public void Add(string key, object value)
    {
        _contextRegistration.Add(key, value);
    }

    public bool ContainsKey(string key)
    {
        return _contextRegistration.ContainsKey(key);
    }

    public void RegisterConstant(string name, object value)
    {
        _contextRegistration.RegisterConstant(name, value);
    }

    public void RegisterAction(string name, Action action)
    {
        _contextRegistration.RegisterAction(name, action);
    }

    public void RegisterAction<T1>(string name, Action<T1> action)
    {
        if (Check<T1>())
        {
            DelegateCustomFunction value = new ScribanModuleWithFunctions.InternalDelegateCustomActionWithInterfaceContext<T, T1>(action);
            _contextRegistration.RegisterVariable(name, value);
            return;
        }

        _contextRegistration.RegisterAction(name, action);
    }

    public void RegisterAction<T1, T2>(string name, Action<T1, T2> action)
    {
        if (Check<T1>())
        {
            DelegateCustomFunction value = new ScribanModuleWithFunctions.InternalDelegateCustomActionWithInterfaceContext<T, T1, T2>(action);
            _contextRegistration.RegisterVariable(name, value);
            return;
        }

        _contextRegistration.RegisterAction(name, action);
    }

    public void RegisterAction<T1, T2, T3>(string name, Action<T1, T2, T3> action)
    {
        if (Check<T1>())
        {
            DelegateCustomFunction value = new ScribanModuleWithFunctions.InternalDelegateCustomActionWithInterfaceContext<T, T1, T2, T3>(action);
            _contextRegistration.RegisterVariable(name, value);
            return;
        }

        _contextRegistration.RegisterAction(name, action);
    }

    public void RegisterAction<T1, T2, T3, T4>(string name, Action<T1, T2, T3, T4> action)
    {
        if (Check<T1>())
        {
            DelegateCustomFunction value = new ScribanModuleWithFunctions.InternalDelegateCustomActionWithInterfaceContext<T, T1, T2, T3, T4>(action);
            _contextRegistration.RegisterVariable(name, value);
            return;
        }

        _contextRegistration.RegisterAction(name, action);
    }

    public void RegisterAction<T1, T2, T3, T4, T5>(string name, Action<T1, T2, T3, T4, T5> action)
    {
        if (Check<T1>())
        {
            DelegateCustomFunction value = new ScribanModuleWithFunctions.InternalDelegateCustomActionWithInterfaceContext<T, T1, T2, T3, T4, T5>(action);
            _contextRegistration.RegisterVariable(name, value);
            return;
        }

        _contextRegistration.RegisterAction(name, action);
    }

    public void RegisterFunction<T1>(string name, Func<T1> func)
    {
        _contextRegistration.RegisterFunction(name, func);
    }

    public void RegisterFunction<T1, T2>(string name, Func<T1, T2> func)
    {
        if (Check<T1>())
        {
            DelegateCustomFunction value = new ScribanModuleWithFunctions.InternalDelegateCustomFunctionWithInterfaceContext<T, T1, T2>(func);
            _contextRegistration.RegisterVariable(name, value);
            return;
        }

        _contextRegistration.RegisterFunction(name, func);
    }

    public void RegisterFunction<T1, T2, T3>(string name, Func<T1, T2, T3> func)
    {
        if (Check<T1>())
        {
            DelegateCustomFunction value = new ScribanModuleWithFunctions.InternalDelegateCustomFunctionWithInterfaceContext<T, T1, T2, T3>(func);
            _contextRegistration.RegisterVariable(name, value);
            return;
        }

        _contextRegistration.RegisterFunction(name, func);
    }

    public void RegisterFunction<T1, T2, T3, T4>(string name, Func<T1, T2, T3, T4> func)
    {
        if (Check<T1>())
        {
            DelegateCustomFunction value = new ScribanModuleWithFunctions.InternalDelegateCustomFunctionWithInterfaceContext<T, T1, T2, T3, T4>(func);
            _contextRegistration.RegisterVariable(name, value);
            return;
        }

        _contextRegistration.RegisterFunction(name, func);
    }

    public void RegisterFunction<T1, T2, T3, T4, T5>(string name, Func<T1, T2, T3, T4, T5> func)
    {
        if (Check<T1>())
        {
            DelegateCustomFunction value = new ScribanModuleWithFunctions.InternalDelegateCustomFunctionWithInterfaceContext<T, T1, T2, T3, T4, T5>(func);
            _contextRegistration.RegisterVariable(name, value);
            return;
        }
        
        _contextRegistration.RegisterFunction(name, func);
    }

    public void RegisterFunction<T1, T2, T3, T4, T5, T6>(string name, Func<T1, T2, T3, T4, T5, T6> func)
    {
        if (Check<T1>())
        {
            DelegateCustomFunction value = new ScribanModuleWithFunctions.InternalDelegateCustomFunctionWithInterfaceContext<T, T1, T2, T3, T4, T5, T6>(func);
            _contextRegistration.RegisterVariable(name, value);
            return;
        }

        _contextRegistration.RegisterFunction(name, func);
    }

    public void RegisterVariable(string name, object value)
    {
        _contextRegistration.RegisterVariable(name, value);
    }

    private static bool Check<T1>()
    {
        var isInterface = typeof(T1).IsInterface;
        var isAssignableFrom = typeof(T1).IsAssignableFrom(typeof(T));
        return isInterface && isAssignableFrom;
    }
}

#pragma warning restore S2436