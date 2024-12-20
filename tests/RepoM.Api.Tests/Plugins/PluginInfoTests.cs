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
        var sut = new PluginInfo("C:\\path\\abc.dll", new PackageAttribute("my test package", "tooltip of test package", "desc"), [0x12,]);

        // assert
        sut.Name.Should().Be("my test package");
        sut.ToolTip.Should().Be("tooltip of test package");
        sut.Description.Should().Be("desc");
        sut.Hash.Should().BeEquivalentTo(new byte[] { 0x12, });
        sut.AssemblyPath.Should().Be("C:\\path\\abc.dll");
    }
}