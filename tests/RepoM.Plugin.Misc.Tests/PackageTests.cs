namespace RepoM.Plugin.Misc.Tests;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using FakeItEasy;
using FluentAssertions;
using RepoM.Core.Plugin;
using Xunit;

public class PackageTests
{
    public static IEnumerable<object[]> PackagesTestData => PluginStore.Packages.Select(package => new object[] { package, });

    [Theory]
    [MemberData(nameof(PackagesTestData))]
    public void Name_ShouldNotBeNullOrWhiteSpace(IPackage package)
    {
        package.Name.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void Name_ShouldBeUnique()
    {
        PluginStore.Packages.Should().OnlyHaveUniqueItems(package => package.Name);
    }
}