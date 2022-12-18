namespace RepoM.Api.IO;

using RepoM.Api.Common;
using RepoM.Api.Git;
using RepoM.Core.Plugin.Expressions;

public static class NameHelper
{
    // todo
    public static string EvaluateName(in string? input, in Repository repository, ITranslationService translationService, IRepositoryExpressionEvaluator repositoryExpressionEvaluator)
    {
        return repositoryExpressionEvaluator.EvaluateStringExpression(
            ReplaceTranslatables(
                translationService.Translate(input ?? string.Empty),
                translationService),
            repository);
    }

    private static string ReplaceTranslatables(string? value, ITranslationService translationService)
    {
        if (value is null)
        {
            return string.Empty;
        }

        value = ReplaceTranslatable(value, "Open", translationService);
        value = ReplaceTranslatable(value, "OpenIn", translationService);
        value = ReplaceTranslatable(value, "OpenWith", translationService);

        return value;
    }

    private static string ReplaceTranslatable(string value, string translatable, ITranslationService translationService)
    {
        if (!value.StartsWith("{" + translatable + "}"))
        {
            return value;
        }

        var rest = value.Replace("{" + translatable + "}", "").Trim();
        return translationService.Translate("(" + translatable + ")", rest); // XMl doesn't support {}

    }
}