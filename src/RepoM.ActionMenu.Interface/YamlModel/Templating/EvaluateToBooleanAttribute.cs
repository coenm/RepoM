namespace RepoM.ActionMenu.Interface.YamlModel.Templating;

using System;

[AttributeUsage(AttributeTargets.Property)]
public sealed class EvaluateToBooleanAttribute : EvaluateToAttribute
{
    public EvaluateToBooleanAttribute(bool defaultValue)
    {
        DefaultValue = defaultValue;
    }

    public bool DefaultValue { get; }
}