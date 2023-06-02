namespace RepoM.Api.Tests.IO.VariableProviders;

using System;
using FluentAssertions;
using RepoM.Api.IO.VariableProviders;
using RepoM.Core.Plugin.Repository;
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

    [Fact]
    public void Provide_ShouldReturnValue_WhenAvailableInEnvironment()
    {
        // arrange
        var dynamicEnvironmentKey = $"RepoM_test_{Guid.NewGuid()}";
        Environment.SetEnvironmentVariable(dynamicEnvironmentKey, "test value");
        var sut = new CustomEnvironmentVariableVariableProvider();

        // act
        var result = sut.Provide($"env.{dynamicEnvironmentKey}", null!);

        // assert
        // assert
        result.Should().Be("test value");
    }

    [Fact]
    public void Provide_ShouldReturnValue_WhenAvailableInEnvironmentAndNoContextGiven()
    {
        // arrange
        RepositoryContext context = null!;
        var dynamicEnvironmentKey = $"RepoM_test_{Guid.NewGuid()}";
        Environment.SetEnvironmentVariable(dynamicEnvironmentKey, "test value");
        var sut = new CustomEnvironmentVariableVariableProvider();

        // act
        var result = sut.Provide(context, $"env.{dynamicEnvironmentKey}", null!);

        // assert
        // assert
        result.Should().Be("test value");
    }

    [Fact]
    public void Provide_ShouldReturnStringEmpty_WhenKeyNotAvailableInEnvironment()
    {
        // arrange
        var dynamicEnvironmentKey = $"RepoM_test_{Guid.NewGuid()}";
        var sut = new CustomEnvironmentVariableVariableProvider();

        // assume
        Environment.GetEnvironmentVariables().Contains(dynamicEnvironmentKey).Should().BeFalse();

        // act
        var result = sut.Provide($"env.{dynamicEnvironmentKey}", null!);

        // assert
        result.Should().Be(string.Empty);
    }
}