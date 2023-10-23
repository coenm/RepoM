namespace RepoM.ActionMenu.Interface.YamlModel.Templating;

using System;

[AttributeUsage(AttributeTargets.Property)]
public sealed class VariableAttribute : EvaluateToAttribute
{
    public VariableAttribute()
    {
    }
}