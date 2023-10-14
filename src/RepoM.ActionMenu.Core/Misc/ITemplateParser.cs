namespace RepoM.ActionMenu.Core.Misc;

using Scriban;

internal interface ITemplateParser
{
    Template ParseScriptOnly(string text);

    Template ParseMixed(string text);
}