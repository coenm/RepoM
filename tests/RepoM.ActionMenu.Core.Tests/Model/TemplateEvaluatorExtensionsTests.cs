namespace RepoM.ActionMenu.Core.Tests.Model;

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
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
    
    public static IEnumerable<object[]> EvaluateBooleanScenarios
    {
        get
        {
            foreach (var defaultValue in new[] { true, false, })
            {
                yield return Create("true", defaultValue, true);
                yield return Create("false", defaultValue, false);

                yield return Create(0, defaultValue, false);
                yield return Create(1, defaultValue, true);
                yield return Create(100, defaultValue, true);

                yield return Create(true, defaultValue, true);
                yield return Create(false, defaultValue, false);

                yield return Create(null!, defaultValue, defaultValue);
            }

            static object[] Create(object evaluateValue, bool defaultValue, bool expectedResult)
            {
                return [evaluateValue, defaultValue, expectedResult,];
            }
        }
    }

    [Theory]
    [MemberData(nameof(EvaluateBooleanScenarios))]
    public async Task EvaluateToBooleanAsync_ShouldReturnBooleanBasedOnOutput(object templateEvaluateOutput, bool defaultValue, bool expectedResult)
    {
        // arrange
        A.CallTo(() => _instance.EvaluateAsync("T3xt")).Returns(Task.FromResult(templateEvaluateOutput));

        // act
        var result = await Sut.EvaluateToBooleanAsync(_instance, "T3xt", defaultValue);

        // assert
        result.Should().Be(expectedResult);
    }
}
