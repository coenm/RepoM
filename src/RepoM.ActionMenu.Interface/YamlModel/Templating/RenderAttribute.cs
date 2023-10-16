namespace RepoM.ActionMenu.Interface.YamlModel.Templating;

using System;

[AttributeUsage(AttributeTargets.Property)]
public sealed class RenderAttribute : EvaluateToAttribute
{
    public RenderAttribute() : this(string.Empty)
    {
    }

    public RenderAttribute(string defaultValue)
    {
        DefaultValue = defaultValue;
    }

    public string DefaultValue { get; }
}