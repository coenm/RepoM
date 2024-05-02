namespace RepoM.App.ViewModels;

using System;
using System.Reflection;
using JetBrains.Annotations;
using RepoM.Api.Common;
using RepoM.Api.Git;

public class HelpViewModel
{
    public HelpViewModel(ITranslationService translationService)
    {
        ArgumentNullException.ThrowIfNull(translationService);
        Header = GetHeader();
        Description = GetHelp(translationService);
    }

    public string Header { [UsedImplicitly] get; }

    public string Description { [UsedImplicitly] get; }

    private static string GetHeader()
    {
        AssemblyName? appName = Assembly.GetEntryAssembly()?.GetName();
        return appName?.Name + ' ' + appName?.Version?.ToString(2).Trim();
    }

    private static string GetHelp(ITranslationService translationService)
    {
        return translationService.Translate(
            "Help Detail",
            StatusCharacterMap.IDENTICAL_SIGN,
            StatusCharacterMap.STASH_SIGN,
            StatusCharacterMap.IDENTICAL_SIGN,
            StatusCharacterMap.ARROW_UP_SIGN,
            StatusCharacterMap.ARROW_DOWN_SIGN,
            StatusCharacterMap.NO_UPSTREAM_SIGN,
            StatusCharacterMap.STASH_SIGN
        );
    }
}