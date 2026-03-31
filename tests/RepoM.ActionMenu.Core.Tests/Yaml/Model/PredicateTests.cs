namespace RepoM.ActionMenu.Core.Tests.Yaml.Model;

using System.Threading.Tasks;
using AwesomeAssertions;
using FakeItEasy;
using RepoM.ActionMenu.Interface.ActionMenuFactory;
using RepoM.ActionMenu.Interface.YamlModel.Templating;
using Xunit;

public class PredicateTests
{
    [Fact]
    public void ImplicitOperatorBool_True_ShouldSetValueAndStaticValue()
    {
        // act
        Predicate sut = true;

        // assert
        sut.Value.Should().Be("true");
    }

    [Fact]
    public void ImplicitOperatorBool_False_ShouldSetValueAndStaticValue()
    {
        // act
        Predicate sut = false;

        // assert
        sut.Value.Should().Be("false");
    }

    [Fact]
    public async Task EvaluateAsync_WhenCreatedFromBoolTrue_ShouldReturnTrue()
    {
        // arrange
        Predicate sut = true;
        var evaluator = A.Fake<ITemplateEvaluator>();

        // act
        var result = await sut.EvaluateAsync(evaluator);

        // assert
        result.Should().BeTrue();
        A.CallTo(evaluator).MustNotHaveHappened();
    }

    [Fact]
    public async Task EvaluateAsync_WhenCreatedFromBoolFalse_ShouldReturnFalse()
    {
        // arrange
        Predicate sut = false;
        var evaluator = A.Fake<ITemplateEvaluator>();

        // act
        var result = await sut.EvaluateAsync(evaluator);

        // assert
        result.Should().BeFalse();
        A.CallTo(evaluator).MustNotHaveHappened();
    }

    [Theory]
    [InlineData("true")]
    [InlineData("True")]
    [InlineData("TRUE")]
    [InlineData("tRuE")]
    public async Task ImplicitOperatorString_TrueVariants_ShouldEvaluateToTrue(string value)
    {
        // arrange
        Predicate sut = value;
        var evaluator = A.Fake<ITemplateEvaluator>();

        // act
        var result = await sut.EvaluateAsync(evaluator);

        // assert
        result.Should().BeTrue();
        A.CallTo(evaluator).MustNotHaveHappened();
    }

    [Theory]
    [InlineData("false")]
    [InlineData("False")]
    [InlineData("FALSE")]
    [InlineData("fAlSe")]
    public async Task ImplicitOperatorString_FalseVariants_ShouldEvaluateToFalse(string value)
    {
        // arrange
        Predicate sut = value;
        var evaluator = A.Fake<ITemplateEvaluator>();

        // act
        var result = await sut.EvaluateAsync(evaluator);

        // assert
        result.Should().BeFalse();
        A.CallTo(evaluator).MustNotHaveHappened();
    }

    [Fact]
    public void ImplicitOperatorString_NonBooleanValue_ShouldSetValue()
    {
        // act
        Predicate sut = "some expression";

        // assert
        sut.Value.Should().Be("some expression");
    }

    [Fact]
    public async Task EvaluateAsync_WhenNonStaticValue_ShouldCallEvaluator()
    {
        // arrange
        Predicate sut = "some expression";
        var evaluator = A.Fake<ITemplateEvaluator>();
        A.CallTo(() => evaluator.EvaluateAsync("some expression")).Returns(Task.FromResult<object>(true));

        // act
        var result = await sut.EvaluateAsync(evaluator);

        // assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task EvaluateAsync_WhenEvaluatorReturnsFalse_ShouldReturnFalse()
    {
        // arrange
        Predicate sut = "some expression";
        var evaluator = A.Fake<ITemplateEvaluator>();
        A.CallTo(() => evaluator.EvaluateAsync("some expression")).Returns(Task.FromResult<object>(false));

        // act
        var result = await sut.EvaluateAsync(evaluator);

        // assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task EvaluateAsync_WhenEvaluatorReturnsNull_ShouldReturnDefaultValue()
    {
        // arrange
        Predicate sut = "some expression";
        sut.DefaultValue = true;
        var evaluator = A.Fake<ITemplateEvaluator>();
        A.CallTo(() => evaluator.EvaluateAsync("some expression")).Returns(Task.FromResult<object>(null!));

        // act
        var result = await sut.EvaluateAsync(evaluator);

        // assert
        result.Should().BeTrue();
    }

    [Theory]
    [InlineData(0, false)]
    [InlineData(1, true)]
    [InlineData(-1, true)]
    [InlineData(42, true)]
    public async Task EvaluateAsync_WhenEvaluatorReturnsInt_ShouldConvertToBool(int returnValue, bool expected)
    {
        // arrange
        Predicate sut = "expr";
        var evaluator = A.Fake<ITemplateEvaluator>();
        A.CallTo(() => evaluator.EvaluateAsync("expr")).Returns(Task.FromResult<object>(returnValue));

        // act
        var result = await sut.EvaluateAsync(evaluator);

        // assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("true", true)]
    [InlineData("True", true)]
    [InlineData("false", false)]
    [InlineData("False", false)]
    public async Task EvaluateAsync_WhenEvaluatorReturnsBoolString_ShouldParse(string returnValue, bool expected)
    {
        // arrange
        Predicate sut = "expr";
        var evaluator = A.Fake<ITemplateEvaluator>();
        A.CallTo(() => evaluator.EvaluateAsync("expr")).Returns(Task.FromResult<object>(returnValue));

        // act
        var result = await sut.EvaluateAsync(evaluator);

        // assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("0", false)]
    [InlineData("1", true)]
    [InlineData("-1", true)]
    [InlineData("42", true)]
    public async Task EvaluateAsync_WhenEvaluatorReturnsIntString_ShouldConvertToBool(string returnValue, bool expected)
    {
        // arrange
        Predicate sut = "expr";
        var evaluator = A.Fake<ITemplateEvaluator>();
        A.CallTo(() => evaluator.EvaluateAsync("expr")).Returns(Task.FromResult<object>(returnValue));

        // act
        var result = await sut.EvaluateAsync(evaluator);

        // assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task EvaluateAsync_WhenEvaluatorReturnsUnparsableString_ShouldReturnDefaultValue(bool defaultValue)
    {
        // arrange
        Predicate sut = "expr";
        sut.DefaultValue = defaultValue;
        var evaluator = A.Fake<ITemplateEvaluator>();
        A.CallTo(() => evaluator.EvaluateAsync("expr")).Returns(Task.FromResult<object>("not a bool or int"));

        // act
        var result = await sut.EvaluateAsync(evaluator);

        // assert
        result.Should().Be(defaultValue);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task EvaluateAsync_WhenEvaluatorReturnsUnknownType_ShouldReturnDefaultValue(bool defaultValue)
    {
        // arrange
        Predicate sut = "expr";
        sut.DefaultValue = defaultValue;
        var evaluator = A.Fake<ITemplateEvaluator>();
        A.CallTo(() => evaluator.EvaluateAsync("expr")).Returns(Task.FromResult<object>(3.14));

        // act
        var result = await sut.EvaluateAsync(evaluator);

        // assert
        result.Should().Be(defaultValue);
    }

    [Fact]
    public void DefaultValue_ShouldBeFalseByDefault()
    {
        // arrange & act
        var sut = new Predicate();

        // assert
        sut.DefaultValue.Should().BeFalse();
    }

    [Fact]
    public void ToString_ShouldContainPredicateAndDefaultValue()
    {
        // arrange
        Predicate sut = "some expr";
        sut.DefaultValue = true;

        // act
        var result = sut.ToString();

        // assert
        result.Should().Contain("Predicate");
        result.Should().Contain("True");
    }

    [Fact]
    public void ToString_WhenValueIsShort_ShouldContainFullValue()
    {
        // arrange
        Predicate sut = "short";

        // act
        var result = sut.ToString();

        // assert
        result.Should().Contain("short");
    }

    [Fact]
    public void ToString_WhenValueIsLong_ShouldTruncateValue()
    {
        // arrange
        Predicate sut = "this is a very long expression value";

        // act
        var result = sut.ToString();

        // assert
        result.Should().Contain("this is a ..");
        result.Should().NotContain("this is a very long expression value");
    }
}
