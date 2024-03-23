namespace RepoM.ActionMenu.Core.Tests.Model;

using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using System.Xml.XPath;
using FakeItEasy;
using FluentAssertions;
using RepoM.ActionMenu.Interface.ActionMenuFactory;
using Xunit;
using Sut = Core.Model.TemplateEvaluatorExtensions;

[SuppressMessage("ReSharper", "InvokeAsExtensionMethod", Justification = "ExtensionClass is sut")]
public class TemplateEvaluatorExtensionsTests
{
    private readonly ITemplateEvaluator _instance;

    public TemplateEvaluatorExtensionsTests()
    {
        _instance = A.Fake<ITemplateEvaluator>();
    }

    [Fact]
    public async Task RenderNullableString_ShouldReturnEmptyString_WhenArgumentIsNull()
    {
        // arrange

        // act
        var result = await Sut.RenderNullableString(_instance, null);

        // assert
        result.Should().Be(string.Empty);
        A.CallTo(_instance).MustNotHaveHappened();
    }

    [Fact]
    public async Task RenderNullableString_ShouldReturnRenderedString_WhenArgumentIsNotNull()
    {
        // arrange
        var txt = "text";
        A.CallTo(() => _instance.RenderStringAsync(txt)).Returns(Task.FromResult("DUMMY RESULT"));

        // act
        var result = await Sut.RenderNullableString(_instance, txt);

        // assert
        result.Should().Be("DUMMY RESULT");
        A.CallTo(_instance).MustHaveHappenedOnceExactly();
    }

    [Theory]
    [InlineData(null, false)]
    [InlineData(null, true)]
    [InlineData(" ", false)]
    [InlineData(" ", true)]
    public async Task EvaluateToBooleanAsync_ShouldReturnDefaultValue_WhenInputIsNull(string? input, bool defaultValue)
    {
        // arrange

        // act
        var result = await Sut.EvaluateToBooleanAsync(_instance, input, defaultValue);

        // assert
        result.Should().Be(defaultValue);
    }
}