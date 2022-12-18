namespace RepoM.Core.Plugin.Expressions;

using RepoM.Core.Plugin.Repository;

public interface IRepositoryExpressionEvaluator
{
    string EvaluateStringExpression(string value, params IRepository[] repository);

    object? EvaluateValueExpression(string value, params IRepository[] repository);

    bool EvaluateBooleanExpression(string? value, IRepository? repository);
}