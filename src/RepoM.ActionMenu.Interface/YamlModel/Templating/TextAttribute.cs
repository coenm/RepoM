namespace RepoM.ActionMenu.Interface.YamlModel.Templating;

using System;

[AttributeUsage(AttributeTargets.Property)]
public sealed class TextAttribute : EvaluateToAttribute
{
    public TextAttribute() : this(string.Empty)
    {
    }

    public TextAttribute(string defaultValue)
    {
        DefaultValue = defaultValue;
    }

    public string DefaultValue { get; }
}