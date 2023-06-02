namespace RepoM.Api.Tests.IO.VariableProviders;

using RepoM.Api.IO.VariableProviders;
using FluentAssertions;
using Xunit;

public class RepoMVariableProviderTest
{
    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    [InlineData("dummy")]
    [InlineData("abc.def")]
    [InlineData("env.")] // too short
    [InlineData(" var.abc")] // starts with whitespace
    [InlineData("var.")] // too short
    [InlineData("var.   ")] // whitespaces after prefix
    public void CanProvide_ShouldReturnFalse_WhenKeyIsNoMatch(string? key)
    {
        // arrange
        var sut = new RepoMVariableProvider();

        // act
        var result = sut.CanProvide(key!);

        // assert
        result.Should().BeFalse();
    }

    [Theory]
    [InlineData("var.x")]
    [InlineData("vAr.x")] // case insensitive
    public void CanProvide_ShouldReturnTrue_WhenKeyIsMatch(string key)
    {
        // arrange
        var sut = new RepoMVariableProvider();

        // act
        var result = sut.CanProvide(key);

        // assert
        result.Should().BeTrue();
    }
}