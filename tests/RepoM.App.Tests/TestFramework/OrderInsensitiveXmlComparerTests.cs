namespace RepoM.App.Tests.TestFramework;

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AwesomeAssertions;
using Xunit;

public class OrderInsensitiveXmlComparerTests
{
    [Fact]
    public void Normalize_ShouldSortAttributesAndChildElementsAlphabetically()
    {
        var document = OrderInsensitiveXmlComparer.Normalize(
            "<root z=\"2\" a=\"1\"><second /><first /></root>");

        document.Root!.Attributes().Select(attribute => attribute.Name.LocalName)
            .Should().Equal("a", "z");
        document.Root.Elements().Select(element => element.Name.LocalName)
            .Should().Equal("first", "second");
    }

    [Fact]
    public async Task CompareAsync_ShouldIgnoreElementAndAttributeOrder()
    {
        var result = await OrderInsensitiveXmlComparer.CompareAsync(
            "<root z=\"2\" a=\"1\"><second /><first /></root>",
            "<root a=\"1\" z=\"2\"><first /><second /></root>",
            new Dictionary<string, object>());

        result.IsEqual.Should().BeTrue();
    }

    [Fact]
    public async Task CompareAsync_ShouldIgnoreWindowPositionAcrossDisplays()
    {
        var result = await OrderInsensitiveXmlComparer.CompareAsync(
            "<WorktreePreviewWindow Top=\"-10\" Left=\"282\" xmlns:materialDesign=\"http://materialdesigninxaml.net/winfx/xaml/themes\" xmlns:swb=\"clr-namespace:System.Windows.Baml2006;assembly=PresentationFramework\"><Second /><First /></WorktreePreviewWindow>",
            "<WorktreePreviewWindow xmlns:swb=\"clr-namespace:System.Windows.Baml2006;assembly=PresentationFramework\" Left=\"1490\" Top=\"326\" xmlns:materialDesign=\"http://materialdesigninxaml.net/winfx/xaml/themes\"><First /><Second /></WorktreePreviewWindow>",
            new Dictionary<string, object>());

        result.IsEqual.Should().BeTrue();
    }

    [Fact]
    public async Task CompareAsync_ShouldStillDetectDifferentValues()
    {
        var result = await OrderInsensitiveXmlComparer.CompareAsync(
            "<root><item value=\"1\" /></root>",
            "<root><item value=\"2\" /></root>",
            new Dictionary<string, object>());

        result.IsEqual.Should().BeFalse();
    }
}
