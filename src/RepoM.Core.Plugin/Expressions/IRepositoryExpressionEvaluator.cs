namespace RepoM.Core.Plugin.Expressions;

using RepoM.Core.Plugin.Repository;

public interface IRepositoryExpressionEvaluator
{
    string EvaluateStringExpression(string value, IRepository? repository);

    object? EvaluateValueExpression(string value, IRepository? repository);

    bool EvaluateBooleanExpression(string? value, IRepository? repository);
}

public static class RepositoryExpressionEvaluatorExtensions
{
    public static string EvaluateNullStringExpression(this IRepositoryExpressionEvaluator evaluator, string? value, IRepository? repository)
    {
        if (string.IsNullOrEmpty(value))
        {
            return string.Empty;
        }

        return evaluator.EvaluateStringExpression(value, repository);
    }
}