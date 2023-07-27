namespace RepoM.Plugin.Misc.Tests;

using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using RepoM.Core.Plugin;
using RepoM.Plugin.AzureDevOps;
using RepoM.Plugin.Clipboard;
using RepoM.Plugin.EverythingFileSearch;
using RepoM.Plugin.Heidi;
using RepoM.Plugin.LuceneQueryParser;
using RepoM.Plugin.SonarCloud;
using RepoM.Plugin.Statistics;
using RepoM.Plugin.WindowsExplorerGitInfo;
using Xunit;

public class PackageTests
{
    public static IEnumerable<object[]> PackagesTestData => Packages.Select(package => new object[] { package, });

    private static IEnumerable<IPackage> Packages
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

    [Theory]
    [MemberData(nameof(PackagesTestData))]
    public void Name_ShouldNotBeNullOrWhiteSpace(IPackage package)
    {
        package.Name.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void Name_ShouldBeUnique()
    {
        Packages.Should().OnlyHaveUniqueItems(package => package.Name);
    }
}