namespace RepoM.ActionMenu.Core.Tests.Model;

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
    private const string TEXT = "t3xt";
    private readonly ITemplateEvaluator _instance = A.Fake<ITemplateEvaluator>();

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
        A.CallTo(() => _instance.RenderStringAsync(TEXT)).Returns(Task.FromResult("DUMMY RESULT"));

        // act
        var result = await Sut.RenderNullableString(_instance, TEXT);

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

    public static TheoryData<object, bool, bool> EvaluateToBooleanAsyncScenarios()
    {
        TheoryData<object, bool, bool> testCases = [];

        foreach (var defaultValue in new[] { true, false, })
        {
            AddTestCase("true", defaultValue, true);
            AddTestCase("false", defaultValue, false);

            AddTestCase(0, defaultValue, false);
            AddTestCase(1, defaultValue, true);
            AddTestCase(100, defaultValue, true);

            AddTestCase(true, defaultValue, true);
            AddTestCase(false, defaultValue, false);

            AddTestCase(null!, defaultValue, defaultValue);
        }

        return testCases;

        void AddTestCase(object evaluateValue, bool defaultValue, bool expectedResult)
        {
            testCases.Add(evaluateValue, defaultValue, expectedResult);
        }
    }

    [Theory]
    [MemberData(nameof(EvaluateToBooleanAsyncScenarios))]
    public async Task EvaluateToBooleanAsync_ShouldReturnBooleanBasedOnOutput(object templateEvaluateOutput, bool defaultValue, bool expectedResult)
    {
        // arrange
        A.CallTo(() => _instance.EvaluateAsync(TEXT)).Returns(Task.FromResult(templateEvaluateOutput));

        // act
        var result = await Sut.EvaluateToBooleanAsync(_instance, TEXT, defaultValue);

        // assert
        result.Should().Be(expectedResult);
    }
}
