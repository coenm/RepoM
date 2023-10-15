namespace RepoM.ActionMenu.Interface.YamlModel.Templating;

using System;

[AttributeUsage(AttributeTargets.Property)]
public class EvaluateScriptAttribute : Attribute
{
}

public abstract class EvaluateToAttribute : Attribute
{
}