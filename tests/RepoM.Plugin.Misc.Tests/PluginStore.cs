namespace RepoM.Plugin.Misc.Tests;

using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using RepoM.Core.Plugin;
using RepoM.Plugin.AzureDevOps;
using RepoM.Plugin.Clipboard;
using RepoM.Plugin.EverythingFileSearch;
using RepoM.Plugin.Heidi;
using RepoM.Plugin.LuceneQueryParser;
using RepoM.Plugin.SonarCloud;
using RepoM.Plugin.Statistics;
using RepoM.Plugin.WindowsExplorerGitInfo;

internal static class PluginStore
{
    public static IEnumerable<IPackage> Packages
    {
        get
        {
            yield return new AzureDevOpsPackage();
            yield return new ClipboardPackage();
            yield return new EverythingPackage();
            yield return new HeidiPackage();
            yield return new LuceneQueryParserPackage();
            yield return new SonarCloudPackage();
            yield return new StatisticsPackage();
            yield return new WindowsExplorerGitInfoPackage();
        }
    }

    public static IEnumerable<Assembly> Assemblies => Packages.Select(package => package.GetType().Assembly).Distinct();
}