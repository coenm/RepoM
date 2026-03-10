namespace RepoM.App.Services;

using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Core;

internal static class LoggingFactory
{
    public static IConfiguration CreateLoggerConfiguration(string appDataPath)
    {
        const string FILENAME = "appsettings.serilog.json";
        var fullFilename = Path.Combine(appDataPath, FILENAME);

        IConfigurationBuilder builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile(fullFilename, optional: true, reloadOnChange: false)
            .AddEnvironmentVariables();
        return builder.Build();
    }

    public static ILoggerFactory CreateLoggerFactory(IConfiguration config)
    {
        ILoggerFactory loggerFactory = new LoggerFactory();

        LoggerConfiguration loggerConfiguration = new LoggerConfiguration()
            .Enrich.WithThreadId()
            .Enrich.WithThreadName()
            .Enrich.WithProperty("ThreadName", "BG")
            .ReadFrom.Configuration(config);

        Logger logger = loggerConfiguration.CreateLogger();

        _ = loggerFactory.AddSerilog(logger);

        return loggerFactory;
    }
}
