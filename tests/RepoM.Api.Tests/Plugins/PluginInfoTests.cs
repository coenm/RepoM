namespace RepoM.Api.Tests.Plugins;

using FluentAssertions;
using RepoM.Api.Plugins;
using RepoM.Core.Plugin.AssemblyInformation;
using Xunit;

public class PluginInfoTests
{
    [Fact]
    public void Ctor_ShouldInitializeReadOnlyProperties()
    {
        // arrange

        // act
        var sut = new PluginInfo("C:\\path\\abc.dll", new PackageAttribute("my test package", "description of test package"), new byte[] { 0x12, });

        // assert
        sut.Name.Should().Be("my test package");
        sut.Description.Should().Be("description of test package");
        sut.Hash.Should().BeEquivalentTo(new byte[] { 0x12, });
        sut.AssemblyPath.Should().Be("C:\\path\\abc.dll");
    }
}