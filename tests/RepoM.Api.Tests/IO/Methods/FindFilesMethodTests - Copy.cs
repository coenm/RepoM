namespace RepoM.Api.Tests.IO.Methods;

using RepoM.Api.IO.Methods;
using FluentAssertions;
using Xunit;

public class IsNullMethodTests
{
    private readonly IsNullMethod _sut = new();

    [Theory]
    [InlineData("IsNull", true)]
    [InlineData("isnull", true)]
    [InlineData("IsNullx", false)]
    [InlineData("", false)]
    [InlineData(null, false)]
    public void CanHandle_ShouldReturnExpected(string? method, bool expectedResult)
    {
        // arrange

        // act
        var result = _sut.CanHandle(method!);

        // assert
        _ = result.Should().Be(expectedResult);
    }

    [Fact]
    public void Handle_ShouldReturnTrue_WhenAnArgumentsIsNull1()
    {
        // arrange

        // act
        var result = _sut.Handle("IsNull", null!);

        // assert
        _ = result.Should().BeOfType<bool>();
        _ = result.Should().Be(true);
    }

    [Fact]
    public void Handle_ShouldReturnTrue_WhenAnArgumentsIsNull2()
    {
        // arrange

        // act
        var result = _sut.Handle("IsNull", new object?[] { null!, });

        // assert
        _ = result.Should().BeOfType<bool>();
        _ = result.Should().Be(true);
    }

    [Fact]
    public void Handle_ShouldReturnTrue_WhenAnArgumentsIsNull3()
    {
        // arrange

        // act
        var result = _sut.Handle("IsNull", null!, null!);

        // assert
        _ = result.Should().BeOfType<bool>();
        _ = result.Should().Be(true);
    }

    [Theory]
    [InlineData("string")]
    [InlineData(12)]
    [InlineData(true)] 
    [InlineData(false)] 
    public void Handle_ShouldReturnTrue_WhenAnArgumentsIsNull4(object notNullValue)
    {
        // arrange

        // act
        var result = _sut.Handle("IsNull", null!, notNullValue, null!);

        // assert
        _ = result.Should().BeOfType<bool>();
        _ = result.Should().Be(true);
    }


    [Theory]
    [InlineData("string")]
    [InlineData(12)]
    [InlineData(true)]
    [InlineData(false)]
    public void Handle_ShouldReturnFalse_WhenAllArgumentsAreNotNull1(object notNullValue)
    {
        // arrange

        // act
        var result = _sut.Handle("IsNull", notNullValue);

        // assert
        _ = result.Should().BeOfType<bool>();
        _ = result.Should().Be(false);
    }

    [Theory]
    [InlineData("string")]
    [InlineData(12)]
    [InlineData(true)]
    [InlineData(false)]
    public void Handle_ShouldReturnFalse_WhenAllArgumentsAreNotNull2(object notNullValue)
    {
        // arrange

        // act
        var result = _sut.Handle("IsNull", "first param", notNullValue);

        // assert
        _ = result.Should().BeOfType<bool>();
        _ = result.Should().Be(false);
    }
}