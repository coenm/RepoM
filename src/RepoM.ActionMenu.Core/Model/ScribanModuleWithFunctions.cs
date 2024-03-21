namespace RepoM.ActionMenu.Core.Model;

using System;
using Scriban;
using Scriban.Parsing;
using Scriban.Runtime;

internal abstract class ScribanModuleWithFunctions : RepoMScriptObject
{
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
        RegisterCustomFunction(name, DelegateCustomFunction.CreateFunc(func));
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
        ArgumentNullException.ThrowIfNull(name);
        ArgumentNullException.ThrowIfNull(value);

        var names = name.Split(',');

        foreach (var subName in names)
        {
            SetValue(subName, value, true);
        }
    }

    /// <summary>
    /// A custom function taking 1 argument.
    /// </summary>
    public class InternalDelegateCustomFunctionWithInterfaceContext<T, T1, TResult> : DelegateCustomFunction where T : TemplateContext
    {
        public InternalDelegateCustomFunctionWithInterfaceContext(Func<T1, TResult> func)
            : base(RewriteFunc<T/*, T1*/>(func))
        // : base(RewriteFunc<ActionMenuGenerationContext, IActionMenuGenerationContext>(func))
        {
            Func = func;
        }

        private static Delegate RewriteFunc<TTemplateContext/*, TContextInterface*/>(Func<T1, TResult> func)
            where TTemplateContext : TemplateContext/*, TContextInterface*/
        {
            // todo implemenets
            //if (typeof(T1) == typeof(TContextInterface))
            {
                // Probably does not matter as Func is used to invoke, only important thing is the signature of the method.
                return (TTemplateContext arg1) => func((T1)(object)arg1);
            }

            return func;
        }

        public Func<T1, TResult> Func { get; }

        protected override object InvokeImpl(TemplateContext context, SourceSpan span, object[] arguments)
        {
            var arg1 = (T1)arguments[0];
            return Func(arg1);
        }
    }

    /// <summary>
    /// A custom function taking 2 arguments.
    /// </summary>
    public class InternalDelegateCustomFunctionWithInterfaceContext<T, T1, T2, TResult> : DelegateCustomFunction where T : TemplateContext
    {
        public InternalDelegateCustomFunctionWithInterfaceContext(Func<T1, T2, TResult> func)
            : base(RewriteFunc<T/*, T1*/>(func))
        // : base(RewriteFunc<ActionMenuGenerationContext, IActionMenuGenerationContext>(func))
        {
            Func = func;
        }

        private static Delegate RewriteFunc<TTemplateContext/*, TContextInterface*/>(Func<T1, T2, TResult> func)
            where TTemplateContext : TemplateContext/*, TContextInterface*/
        {
            // todo implemenets
            //if (typeof(T1) == typeof(TContextInterface))
            {
                // Probably does not matter as Func is used to invoke, only important thing is the signature of the method.
                return (TTemplateContext arg1, T2 arg2) => func((T1)(object)arg1, arg2);
            }

            return func;
        }

        public Func<T1, T2, TResult> Func { get; }

        protected override object InvokeImpl(TemplateContext context, SourceSpan span, object[] arguments)
        {
            var arg1 = (T1)arguments[0];
            var arg2 = (T2)arguments[1];
            return Func(arg1, arg2);
        }
    }

    /// <summary>
    /// A custom function taking 3 arguments.
    /// </summary>
    public class InternalDelegateCustomFunctionWithInterfaceContext<T, T1, T2, T3, TResult> : DelegateCustomFunction where T : TemplateContext
    {
        public InternalDelegateCustomFunctionWithInterfaceContext(Func<T1, T2, T3, TResult> func)
            : base(RewriteFunc<T/*, T1*/>(func))
        // : base(RewriteFunc<ActionMenuGenerationContext, IActionMenuGenerationContext>(func))
        {
            Func = func;
        }

        private static Delegate RewriteFunc<TTemplateContext/*, TContextInterface*/>(Func<T1, T2, T3, TResult> func)
            where TTemplateContext : TemplateContext/*, TContextInterface*/
        {
            // todo implemenets
            //if (typeof(T1) == typeof(TContextInterface))
            {
                // Probably does not matter as Func is used to invoke, only important thing is the signature of the method.
                return (TTemplateContext arg1, T2 arg2, T3 arg3) => func((T1)(object)arg1, arg2, arg3);
            }

            return func;
        }

        public Func<T1, T2, T3, TResult> Func { get; }

        protected override object InvokeImpl(TemplateContext context, SourceSpan span, object[] arguments)
        {
            var arg1 = (T1)arguments[0];
            var arg2 = (T2)arguments[1];
            var arg3 = (T3)arguments[2];
            return Func(arg1, arg2, arg3);
        }
    }


    /// <summary>
    /// A custom function taking 4 arguments.
    /// </summary>
    public class InternalDelegateCustomFunctionWithInterfaceContext<T, T1, T2, T3, T4, TResult> : DelegateCustomFunction where T : TemplateContext
    {
        public InternalDelegateCustomFunctionWithInterfaceContext(Func<T1, T2, T3, T4, TResult> func)
            : base(RewriteFunc<T/*, T1*/>(func))
        // : base(RewriteFunc<ActionMenuGenerationContext, IActionMenuGenerationContext>(func))
        {
            Func = func;
        }

        private static Delegate RewriteFunc<TTemplateContext/*, TContextInterface*/>(Func<T1, T2, T3, T4, TResult> func)
            where TTemplateContext : TemplateContext/*, TContextInterface*/
        {
            // todo implemenets
            //if (typeof(T1) == typeof(TContextInterface))
            {
                // Probably does not matter as Func is used to invoke, only important thing is the signature of the method.
                return (TTemplateContext arg1, T2 arg2, T3 arg3, T4 arg4) => func((T1)(object)arg1, arg2, arg3, arg4);
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

    /// <summary>
    /// A custom function taking 5 arguments.
    /// </summary>
    public class InternalDelegateCustomFunctionWithInterfaceContext<T, T1, T2, T3, T4, T5, TResult> : DelegateCustomFunction where T : TemplateContext
    {
        public InternalDelegateCustomFunctionWithInterfaceContext(Func<T1, T2, T3, T4, T5, TResult> func)
            : base(RewriteFunc<T/*, T1*/>(func))
        // : base(RewriteFunc<ActionMenuGenerationContext, IActionMenuGenerationContext>(func))
        {
            Func = func;
        }

        private static Delegate RewriteFunc<TTemplateContext/*, TContextInterface*/>(Func<T1, T2, T3, T4, T5, TResult> func)
            where TTemplateContext : TemplateContext/*, TContextInterface*/
        {
            // todo implemenets
            //if (typeof(T1) == typeof(TContextInterface))
            {
                // Probably does not matter as Func is used to invoke, only important thing is the signature of the method.
                return (TTemplateContext arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5) => func((T1)(object)arg1, arg2, arg3, arg4, arg5);
            }

            return func;
        }

        public Func<T1, T2, T3, T4, T5, TResult> Func { get; }

        protected override object InvokeImpl(TemplateContext context, SourceSpan span, object[] arguments)
        {
            var arg1 = (T1)arguments[0];
            var arg2 = (T2)arguments[1];
            var arg3 = (T3)arguments[2];
            var arg4 = (T4)arguments[3];
            var arg5 = (T5)arguments[4];
            return Func(arg1, arg2, arg3, arg4, arg5);
        }
    }

    /// <summary>
    /// A custom action taking 1 argument.
    /// </summary>
    public class InternalDelegateCustomActionWithInterfaceContext<T, T1> : DelegateCustomFunction where T : TemplateContext
    {
        public InternalDelegateCustomActionWithInterfaceContext(Action<T1> func)
            : base(RewriteFunc<T/*, T1*/>(func))
        {
            Func = func;
        }

        public Action<T1> Func { get; }

        protected override object InvokeImpl(TemplateContext context, SourceSpan span, object[] arguments)
        {
            var arg1 = (T1)arguments[0];
            Func(arg1);
            return null!;
        }

        private static Delegate RewriteFunc<TTemplateContext/*, TContextInterface*/>(Action<T1> func)
            where TTemplateContext : TemplateContext/*, TContextInterface*/
        {
            // todo implemenets
            //if (typeof(T1) == typeof(TContextInterface))
            {
                // Probably does not matter as Func is used to invoke, only important thing is the signature of the method.
                return (TTemplateContext arg1) => func((T1)(object)arg1);
            }

            return func;
        }
    }
    
    /// <summary>
    /// A custom action taking 2 arguments.
    /// </summary>
    public class InternalDelegateCustomActionWithInterfaceContext<T, T1, T2> : DelegateCustomFunction where T : TemplateContext
    {
        public InternalDelegateCustomActionWithInterfaceContext(Action<T1, T2> func)
            : base(RewriteFunc<T/*, T1*/>(func))
        {
            Func = func;
        }

        public Action<T1, T2> Func { get; }

        protected override object InvokeImpl(TemplateContext context, SourceSpan span, object[] arguments)
        {
            var arg1 = (T1)arguments[0];
            var arg2 = (T2)arguments[1];
            Func(arg1, arg2);
            return null!;
        }

        private static Delegate RewriteFunc<TTemplateContext/*, TContextInterface*/>(Action<T1, T2> func)
            where TTemplateContext : TemplateContext/*, TContextInterface*/
        {
            // todo implemenets
            //if (typeof(T1) == typeof(TContextInterface))
            {
                // Probably does not matter as Func is used to invoke, only important thing is the signature of the method.
                return (TTemplateContext arg1, T2 arg2) => func((T1)(object)arg1, arg2);
            }

            return func;
        }
    }

    /// <summary>
    /// A custom action taking 3 arguments.
    /// </summary>
    public class InternalDelegateCustomActionWithInterfaceContext<T, T1, T2, T3> : DelegateCustomFunction where T : TemplateContext
    {
        public InternalDelegateCustomActionWithInterfaceContext(Action<T1, T2, T3> func)
            : base(RewriteFunc<T/*, T1*/>(func))
        {
            Func = func;
        }

        public Action<T1, T2, T3> Func { get; }

        protected override object InvokeImpl(TemplateContext context, SourceSpan span, object[] arguments)
        {
            var arg1 = (T1)arguments[0];
            var arg2 = (T2)arguments[1];
            var arg3 = (T3)arguments[2];
            Func(arg1, arg2, arg3);
            return null!;
        }

        private static Delegate RewriteFunc<TTemplateContext/*, TContextInterface*/>(Action<T1, T2, T3> func)
            where TTemplateContext : TemplateContext/*, TContextInterface*/
        {
            // todo implemenets
            //if (typeof(T1) == typeof(TContextInterface))
            {
                // Probably does not matter as Func is used to invoke, only important thing is the signature of the method.
                return (TTemplateContext arg1, T2 arg2, T3 arg3) => func((T1)(object)arg1, arg2, arg3);
            }

            return func;
        }
    }

    /// <summary>
    /// A custom action taking 4 arguments.
    /// </summary>
    public class InternalDelegateCustomActionWithInterfaceContext<T, T1, T2, T3, T4> : DelegateCustomFunction where T : TemplateContext
    {
        public InternalDelegateCustomActionWithInterfaceContext(Action<T1, T2, T3, T4> func)
            : base(RewriteFunc<T/*, T1*/>(func))
        {
            Func = func;
        }

        public Action<T1, T2, T3, T4> Func { get; }

        protected override object InvokeImpl(TemplateContext context, SourceSpan span, object[] arguments)
        {
            var arg1 = (T1)arguments[0];
            var arg2 = (T2)arguments[1];
            var arg3 = (T3)arguments[2];
            var arg4 = (T4)arguments[3];
            Func(arg1, arg2, arg3, arg4);
            return null!;
        }

        private static Delegate RewriteFunc<TTemplateContext/*, TContextInterface*/>(Action<T1, T2, T3, T4> func)
            where TTemplateContext : TemplateContext/*, TContextInterface*/
        {
            // todo implemenets
            //if (typeof(T1) == typeof(TContextInterface))
            {
                // Probably does not matter as Func is used to invoke, only important thing is the signature of the method.
                return (TTemplateContext arg1, T2 arg2, T3 arg3, T4 arg4) => func((T1)(object)arg1, arg2, arg3, arg4);
            }

            return func;
        }
    }

    /// <summary>
    /// A custom action taking 5 arguments.
    /// </summary>
    public class InternalDelegateCustomActionWithInterfaceContext<T, T1, T2, T3, T4, T5> : DelegateCustomFunction where T : TemplateContext
    {
        public InternalDelegateCustomActionWithInterfaceContext(Action<T1, T2, T3, T4, T5> func)
            : base(RewriteFunc<T/*, T1*/>(func))
        {
            Func = func;
        }

        public Action<T1, T2, T3, T4, T5> Func { get; }

        protected override object InvokeImpl(TemplateContext context, SourceSpan span, object[] arguments)
        {
            var arg1 = (T1)arguments[0];
            var arg2 = (T2)arguments[1];
            var arg3 = (T3)arguments[2];
            var arg4 = (T4)arguments[3];
            var arg5 = (T5)arguments[4];
            Func(arg1, arg2, arg3, arg4, arg5);
            return null!;
        }

        private static Delegate RewriteFunc<TTemplateContext/*, TContextInterface*/>(Action<T1, T2, T3, T4, T5> func)
            where TTemplateContext : TemplateContext/*, TContextInterface*/
        {
            // todo implemenets
            //if (typeof(T1) == typeof(TContextInterface))
            {
                // Probably does not matter as Func is used to invoke, only important thing is the signature of the method.
                return (TTemplateContext arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5) => func((T1)(object)arg1, arg2, arg3, arg4, arg5);
            }

            return func;
        }
    }
}