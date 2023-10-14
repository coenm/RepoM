namespace RepoM.ActionMenu.Core.Model;

using System;
using RepoM.ActionMenu.Interface.ActionMenuFactory;
using Scriban;
using Scriban.Parsing;
using Scriban.Runtime;

internal abstract class ScribanModuleWithFunctions : RepoMScriptObject
{
    // protected KalkModuleWithFunctions() : this(null)
    // {
    // }
    //
    // protected KalkModuleWithFunctions(string? name)
    // {
    //     // Content = new ScriptObject();
    // }

    // public ScriptObject Content { get; }

    // protected override void Import()
    // {
    //     base.Import();
    //
    //     // Feed the engine with our new builtins
    //     Engine.Builtins.Import(Content);
    //
    //     DisplayImport();
    // }
    //
    // protected virtual void DisplayImport()
    // {
    //     if (!IsBuiltin && Content.Count > 0)
    //     {
    //         Engine.WriteHighlightLine($"# {Content.Count} functions successfully imported from module `{Name}`.");
    //     }
    // }

    protected virtual void RegisterFunctions()
    {
        // intentionally do nothing.
    }

    protected void RegisterConstant(string name, object value)
    {
        RegisterVariable(name, value);
    }
    protected void RegisterAction(string name, Action action)
    {
        RegisterCustomFunction(name, DelegateCustomFunction.Create(action));
    }

    protected void RegisterAction<T1>(string name, Action<T1> action)
    {
        RegisterCustomFunction(name, DelegateCustomFunction.Create(action));
    }

    protected void RegisterAction<T1, T2>(string name, Action<T1, T2> action)
    {
        RegisterCustomFunction(name, DelegateCustomFunction.Create(action));
    }

    protected void RegisterAction<T1, T2, T3>(string name, Action<T1, T2, T3> action)
    {
        RegisterCustomFunction(name, DelegateCustomFunction.Create(action));
    }

    protected void RegisterAction<T1, T2, T3, T4>(string name, Action<T1, T2, T3, T4> action)
    {
        RegisterCustomFunction(name, DelegateCustomFunction.Create(action));
    }

    protected void RegisterFunction<T1>(string name, Func<T1> func)
    {
        RegisterCustomFunction(name, DelegateCustomFunction.CreateFunc(func));
    }

    protected void RegisterFunction<T1, T2>(string name, Func<T1, T2> func)
    {
        RegisterCustomFunction(name, DelegateCustomFunction.CreateFunc(func));
    }

    protected void RegisterFunction<T1, T2, T3>(string name, Func<T1, T2, T3> func)
    {
        RegisterCustomFunction(name, DelegateCustomFunction.CreateFunc(func));
    }

    protected void RegisterFunction<T1, T2, T3, T4>(string name, Func<T1, T2, T3, T4> func)
    {
        RegisterCustomFunction(name, DelegateCustomFunction.CreateFunc(func));
    }

    protected void RegisterFunction<T1, T2, T3, T4, T5>(string name, Func<T1, T2, T3, T4, T5> func)
    {
        static DelegateCustomFunction CreateFunc<T1, T2, T3, T4, TResult>(Func<T1, T2, T3, T4, TResult> func)
        {
            if (func == null) throw new ArgumentNullException(nameof(func));
            return new InternalDelegateCustomFunctionWithInterfaceContext<T1, T2, T3, T4, TResult>(func);
        }

        RegisterCustomFunction(name, CreateFunc(func));
    }


    /// <summary>
    /// A custom function taking 4 arguments.
    /// </summary>
    /// <typeparam name="T1">Func 0 arg type</typeparam>
    /// <typeparam name="T2">Func 1 arg type</typeparam>
    /// <typeparam name="T3">Func 2 arg type</typeparam>
    /// <typeparam name="T4">Func 3 arg type</typeparam>
    /// <typeparam name="TResult">Type result</typeparam>
    public class InternalDelegateCustomFunctionWithInterfaceContext<T1, T2, T3, T4, TResult> : DelegateCustomFunction
    {
        public InternalDelegateCustomFunctionWithInterfaceContext(Func<T1, T2, T3, T4, TResult> func) 
            : base(RewriteFunc<ActionMenuGenerationContext, IActionMenuGenerationContext>(func))
        {
            Func = func;
        }

        private static Delegate RewriteFunc<TTemplateContext, TContextInterface>(Func<T1, T2, T3, T4, TResult> func)
            where TTemplateContext : TemplateContext, TContextInterface
        {
            if (typeof(T1) == typeof(TContextInterface))
            {
                return (TTemplateContext arg1, T2 arg2, T3 arg3, T4 arg4) =>
                {
                    // Probably does not matter as Func is used to invoke.
                    throw new Exception();
                    // return func((T1)(object)arg1, arg2, arg3, arg4);
                };
            }

            return func;
        }

        public Func<T1, T2, T3, T4, TResult> Func { get; }

        protected override object InvokeImpl(TemplateContext context, SourceSpan span, object[] arguments)
        {
            var arg1 = (T1)arguments[0];
            var arg2 = (T2)arguments[1];
            var arg3 = (T3)arguments[2];
            var arg4 = (T4)arguments[3];
            return Func(arg1, arg2, arg3, arg4);
        }
    }

    protected void RegisterFunction<T1, T2, T3, T4, T5, T6>(string name, Func<T1, T2, T3, T4, T5, T6> func)
    {
        RegisterCustomFunction(name, DelegateCustomFunction.CreateFunc(func));
    }

    protected void RegisterCustomFunction(string name, IScriptCustomFunction func)
    {
        RegisterVariable(name, func);
    }

    protected void RegisterVariable(string name, object value)
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