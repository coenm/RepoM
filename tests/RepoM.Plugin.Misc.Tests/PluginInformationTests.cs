namespace RepoM.Plugin.Misc.Tests;

using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FluentAssertions;
using RepoM.Core.Plugin.AssemblyInformation;
using Xunit;

public class PluginInformationTests
{
    public static IEnumerable<object[]> AssembliesTestData => PluginStore.Assemblies.Select(assembly => new object[] { assembly, });

    [Theory]
    [MemberData(nameof(AssembliesTestData))]
    public void PluginAssembly_ShouldContainPackageAttributeWithValidValues(Assembly assembly)
    {
        // arrange

        // act
        PackageAttribute? result = assembly.GetCustomAttribute<PackageAttribute>();

        // assert
        Assert.NotNull(result);
        result.Name.Should().NotBeNullOrWhiteSpace();
        result.Description.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void AssemblyName_ShouldBeUnique()
    {
        // arrange

        // act
        IEnumerable<PackageAttribute> packagesInformation = PluginStore.Assemblies.Select(GetFromAssembly);

        // assert
        packagesInformation.Should().OnlyHaveUniqueItems(a => a.Name);
    }

    [Fact]
    public void AssemblyDescription_ShouldBeUnique()
    {
        // arrange

        // act
        IEnumerable<PackageAttribute> packagesInformation = PluginStore.Assemblies.Select(GetFromAssembly);

        // assert
        packagesInformation.Should().OnlyHaveUniqueItems(a => a.Description);
    }

    private static PackageAttribute GetFromAssembly(Assembly assembly)
    {
        PackageAttribute? result = assembly.GetCustomAttribute<PackageAttribute>();
        Assert.NotNull(result);
        return result;
    }
}