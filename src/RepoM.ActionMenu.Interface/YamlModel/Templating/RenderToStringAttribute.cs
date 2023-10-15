namespace RepoM.ActionMenu.Interface.YamlModel.Templating;

using System;

[AttributeUsage(AttributeTargets.Property)]
public sealed class RenderToStringAttribute : EvaluateToAttribute
{
    public RenderToStringAttribute() : this(string.Empty)
    {
    }

    public RenderToStringAttribute(string defaultValue)
    {
        DefaultValue = defaultValue;
    }

    public string DefaultValue { get; }
}