namespace RepoM.ActionMenu.Interface.YamlModel.Templating;

using System;

[AttributeUsage(AttributeTargets.Property)]
public sealed class EvaluateToObjectAttribute : EvaluateToAttribute
{
    public EvaluateToObjectAttribute(Type type)
    {
    }
}

[AttributeUsage(AttributeTargets.Property)]
public sealed class EvaluateToAnyObjectAttribute : EvaluateToAttribute
{
    public EvaluateToAnyObjectAttribute()
    {
    }
}