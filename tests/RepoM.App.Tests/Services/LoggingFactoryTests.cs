namespace RepoM.App.Tests.Services;

using System;
using AwesomeAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RepoM.App.Services;
using Xunit;

public class LoggingFactoryTests
{
    [Fact]
    public void CreateLoggerConfiguration_ShouldReturnConfiguration_WhenConfigFileDoesNotExist()
    {
        // Arrange - use a non-existent path; the JSON file is optional
        var nonExistentPath = @"c:\non\existent\path\" + Guid.NewGuid();

        // Act
        IConfiguration config = LoggingFactory.CreateLoggerConfiguration(nonExistentPath);

        // Assert
        config.Should().NotBeNull();
    }

    [Fact]
    public void CreateLoggerFactory_ShouldReturnLoggerFactory()
    {
        // Arrange
        IConfiguration config = new ConfigurationBuilder().Build();

        // Act
        ILoggerFactory loggerFactory = LoggingFactory.CreateLoggerFactory(config);

        // Assert
        loggerFactory.Should().NotBeNull();
        loggerFactory.Dispose();
    }

    [Fact]
    public void CreateLoggerFactory_ShouldCreateWorkingLogger()
    {
        // Arrange
        IConfiguration config = new ConfigurationBuilder().Build();
        ILoggerFactory loggerFactory = LoggingFactory.CreateLoggerFactory(config);

        // Act
        ILogger logger = loggerFactory.CreateLogger("Test");

        // Assert
        logger.Should().NotBeNull();
        Action act = () => logger.LogInformation("test message");
        act.Should().NotThrow();

        loggerFactory.Dispose();
    }

    [Fact]
    public void CreateLoggerFactory_ShouldRespectSerilogMinimumLevel()
    {
        // Arrange - provide Serilog config via in-memory configuration
        IConfiguration config = new ConfigurationBuilder()
            .AddInMemoryCollection(
            [
                new("Serilog:MinimumLevel", "Warning"),
            ])
            .Build();

        // Act
        ILoggerFactory loggerFactory = LoggingFactory.CreateLoggerFactory(config);
        ILogger logger = loggerFactory.CreateLogger("Test");

        // Assert
        logger.Should().NotBeNull();
        logger.IsEnabled(LogLevel.Warning).Should().BeTrue();
        logger.IsEnabled(LogLevel.Debug).Should().BeFalse();

        loggerFactory.Dispose();
    }
}
