namespace RepoM.Api.IO.Methods;

using System;
using ExpressionStringEvaluator.Methods;
using JetBrains.Annotations;

[UsedImplicitly]
public class IsNullMethod : IMethod
{
    public bool CanHandle(string method)
    {
        return "IsNull".Equals(method, StringComparison.CurrentCultureIgnoreCase);
    }

    public object? Handle(string method, params object?[] args)
    {
        return args == null || Array.Exists(args, arg => arg is null);
    }
}