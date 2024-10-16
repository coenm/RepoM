namespace RepoM.ActionMenu.CodeGen;

using System.Collections.Generic;
using RepoM.ActionMenu.CodeGen.Models;

internal static class Constants
{
    internal static readonly Dictionary<string, TypeInfoDescriptor> TypeInfos = new()
    {
        {
            typeof(RepoM.ActionMenu.Core.Yaml.Model.ActionMenus.ActionMenu).FullName!,
            new TypeInfoDescriptor(nameof(RepoM.ActionMenu.Core.Yaml.Model.ActionMenus.ActionMenu), typeof(RepoM.ActionMenu.Core.Yaml.Model.ActionMenus.ActionMenu).FullName!)
                {
                    Link = "repository_action_types.md#actions",
                }
        },
        {
            typeof(Interface.YamlModel.Templating.Text).FullName!,
            new TypeInfoDescriptor(nameof(Interface.YamlModel.Templating.Text), typeof(Interface.YamlModel.Templating.Text).FullName!)
                {
                    Link = "repository_action_types.md#text",
                }
        },
        {
            typeof(Interface.YamlModel.Templating.Predicate).FullName!,
            new TypeInfoDescriptor(nameof(Interface.YamlModel.Templating.Predicate), typeof(Interface.YamlModel.Templating.Predicate).FullName!)
                {
                    Link = "repository_action_types.md#predicate",
                }
        },
        {
            typeof(Interface.YamlModel.ActionMenus.Context).FullName!,
            new TypeInfoDescriptor(nameof(Interface.YamlModel.ActionMenus.Context), typeof(Interface.YamlModel.ActionMenus.Context).FullName!)
                {
                    Link = "repository_action_types.md#context",
                }
        },
        {
            typeof(Interface.YamlModel.ActionMenus.Context).FullName! + "?",
            new TypeInfoDescriptor(nameof(Interface.YamlModel.ActionMenus.Context), typeof(Interface.YamlModel.ActionMenus.Context).FullName! + "?")
                {
                    Link = "repository_action_types.md#context",
                }
        },
    };

    /// <summary>
    /// Project names of all the projects that are used in the code generation.
    /// </summary>
    public static readonly List<string> Projects =
    [
        "RepoM.ActionMenu.Interface", // this is for the description of the interface types and its members.
        "RepoM.ActionMenu.Core",

        "RepoM.Plugin.AzureDevOps",
        "RepoM.Plugin.Clipboard",
        "RepoM.Plugin.Heidi",
        "RepoM.Plugin.LuceneQueryParser",
        "RepoM.Plugin.SonarCloud",
        "RepoM.Plugin.Statistics",
        "RepoM.Plugin.WebBrowser",
        "RepoM.Plugin.WindowsExplorerGitInfo",
    ];
}