namespace RepoM.ActionMenu.Interface.YamlModel.Templating;

using System;

public sealed class PredicateEvaluationException : Exception
{
    public PredicateEvaluationException(string predicate, Exception innerException)
        : base(CreateMessage(predicate, innerException), innerException)
    {
        PredicateText = predicate;
    }

    public string PredicateText { get; }

    private static string CreateMessage(string predicateText, Exception exception)
    {
        return $"Could not evaluate predicate '{predicateText}' because {exception.Message}";
    }
}