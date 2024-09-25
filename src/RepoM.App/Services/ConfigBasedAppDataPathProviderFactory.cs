namespace RepoM.App.Services;

using System;
using System.IO.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using RepoM.Api.IO;
using RepoM.App.Configuration;

internal class ConfigBasedAppDataPathProviderFactory
{
    private readonly string[] _args;
    private readonly IFileProvider? _fileProvider;
    private readonly IFileSystem _fileSystem;

    public ConfigBasedAppDataPathProviderFactory(string[] args, IFileSystem fileSystem)
    {
        _args = args ?? throw new ArgumentNullException(nameof(args));
        _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
    }

    public ConfigBasedAppDataPathProviderFactory(string[] args, IFileSystem fileSystem, IFileProvider fileProvider)
        :this(args, fileSystem)
    {
        _fileProvider = fileProvider;
    }

    public AppDataPathProvider Create()
    {
        IConfiguration configuration = CreateConfiguration(_args);
        return CreateAppDataPathProvider(configuration);
    }

    private IConfiguration CreateConfiguration(string[] args)
    {
        var builder = new ConfigurationBuilder();
        if (_fileProvider != null)
        {
            builder.SetFileProvider(_fileProvider);
        }

        builder.AddEnvironmentVariables("REPOM_");

        builder.AddJsonFile("appsettings.json", optional: true, reloadOnChange: false);

#if DEBUG
        builder.AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: false);
#endif
        
        builder.AddCommandLine(args);

        return builder.Build();
    }

    private AppDataPathProvider CreateAppDataPathProvider(IConfiguration appDataPathConfiguration)
    {
        var appConfig = new Config();
        appDataPathConfiguration.Bind("App", appConfig);
        var appDataPathConfig = new AppDataPathConfig
        {
            AppSettingsPath = appConfig.AppSettingsPath,
        };

        return new AppDataPathProvider(appDataPathConfig, _fileSystem);
    }
}