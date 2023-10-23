namespace RepoM.ActionMenu.Interface.YamlModel.Templating;

using System;

[AttributeUsage(AttributeTargets.Property)]
public sealed class PredicateAttribute : EvaluateToAttribute
{
    public PredicateAttribute(bool defaultValue)
    {
        DefaultValue = defaultValue;
    }

    public bool DefaultValue { get; }
}