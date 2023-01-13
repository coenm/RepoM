namespace RepoM.Plugin.Heidi.Tests.Internal;

using System;
using FluentAssertions;
using RepoM.Plugin.Heidi.Internal;
using Xunit;

public class HeidiSettingsTests 
{
    [Fact]
    public void ConfigFilename_ShouldReturnDefault_WhenEnvironmentVariableIsUnset()
    {
        // arrange
        using IDisposable _ = EnvironmentVariableManager.SetEnvironmentVariable("REPOM_HEIDI_CONFIG_FILENAME", string.Empty);
        var sut = new HeidiSettings();

        // act
        var result = sut.ConfigFilename;

        // assert
        result.Should().Be("portable_settings.txt");
    }

    [Fact]
    public void ConfigFilename_ShouldReturnEnvironmentVariable_WhenEnvironmentVariableIsSet()
    {
        // arrange
        using IDisposable _ = EnvironmentVariableManager.SetEnvironmentVariable("REPOM_HEIDI_CONFIG_FILENAME", "Dummy@#");
        var sut = new HeidiSettings();

        // act
        var result = sut.ConfigFilename;

        // assert
        result.Should().Be("Dummy@#");
    }
}