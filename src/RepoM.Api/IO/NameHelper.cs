namespace RepoM.Api.IO;

using RepoM.Api.Common;
using RepoM.Api.Git;
using RepoM.Core.Plugin.Expressions;

public static class NameHelper
{
    public static string EvaluateName(in string? input, in Repository repository, ITranslationService translationService, IRepositoryExpressionEvaluator repositoryExpressionEvaluator)
    {
        return input == null ? string.Empty : repositoryExpressionEvaluator.EvaluateStringExpression(input, repository);
    }
}