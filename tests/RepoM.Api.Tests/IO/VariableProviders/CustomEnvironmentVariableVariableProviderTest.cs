namespace RepoM.Api.Tests.IO.VariableProviders;

using FluentAssertions;
using RepoM.Api.IO.VariableProviders;
using Xunit;

public class CustomEnvironmentVariableVariableProviderTest
{
    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    [InlineData("dummy")]
    [InlineData("abc.def")]
    [InlineData("env.")] // too short
    [InlineData(" env.abc")] // starts with whitespace
    [InlineData("Env.")] // too short
    [InlineData("Env.   ")] // whitespaces after prefix
    public void CanProvide_ShouldReturnFalse_WhenKeyIsNoMatch(string? key)
    {
        // arrange
        var sut = new CustomEnvironmentVariableVariableProvider();

        // act
        var result = sut.CanProvide(key!);

        // assert
        result.Should().BeFalse();
    }
}