namespace RepoM.ActionMenu.Interface.YamlModel.Templating;

using System;

[AttributeUsage(AttributeTargets.Property)]
public sealed class RenderToNullableStringAttribute : EvaluateToAttribute
{
    public RenderToNullableStringAttribute(string? defaultValue)
    {
        DefaultValue = defaultValue;
    }

    public string? DefaultValue { get; }
}