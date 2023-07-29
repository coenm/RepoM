namespace RepoM.Plugin.Misc.Tests.Configuration;

using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using DotNetEnv;
using FakeItEasy;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using RepoM.Api.Plugins;
using RepoM.Core.Plugin;
using RepoM.Core.Plugin.Common;
using RepoM.Core.Plugin.RepositoryActions;
using VerifyTests;
using VerifyXunit;
using Xunit;
using YamlDotNet.Serialization;

[UsesVerify]
public class DocsModuleSettingsTests
{
    private readonly IAppDataPathProvider _appDataPathProvider;
    private FileBasedPackageConfiguration _fileBasedPackageConfiguration;
    private MockFileSystem _fileSystem;
    private ILogger _logger;

    public DocsModuleSettingsTests()
    {
        _appDataPathProvider = A.Fake<IAppDataPathProvider>();
        A.CallTo(() => _appDataPathProvider.AppDataPath).Returns("C:\\tmp\\");
        _fileSystem = new MockFileSystem(new J2N.Collections.Generic.Dictionary<string, MockFileData>()
            {
                { "C:\\tmp\\x.tmp", new MockFileData("x") }, // make sure path exists.
            });
        _logger = NullLogger.Instance;
        _fileBasedPackageConfiguration = new FileBasedPackageConfiguration(_appDataPathProvider, _fileSystem, _logger, "dummy");
    }

    public static IEnumerable<object[]> PackagesTestData => PluginStore.Packages.Select(package => new object[] { package, });

    [Fact]
    public async Task VerifyChanges()
    {
        // arrange
        var results = new Dictionary<string, object?>();

        // act
        foreach (IPackage p in PluginStore.Packages)
        {
            var (config, _) = await PersistDefaultConfigAsync(p);
            results.Add(p.GetType().Name, config);
        }

        // assert
        var settings = new VerifySettings();
        await Verifier.Verify(results, settings);
    }

    [Theory]
    [MemberData(nameof(PackagesTestData))]
    public async Task DocsModuleSettings(IPackage package)
    {
        // arrange
        var packageName = package.GetType().Name;

        // act
        (object? config, string? persistedConfig) = await PersistDefaultConfigAsync(package);

        // assert
        var settings = new VerifySettings();
        // settings.AutoVerify();
        settings.UseDirectory("VerifiedDocs");
        settings.UseTextForParameters(package.GetType().Name);
        if (config == null && persistedConfig == null)
        {
            settings.AppendContentAsFile(CreateConfigWithoutSnippetDocumentationMarkdown(), "md", "desc");
            await Verifier.Verify($"No config in {packageName}", settings: settings);
        }
        else
        {
            settings.AppendContentAsFile(CreateConfigWithSnippetDocumentationMarkdown(persistedConfig/*$"Generated_DefaultConfig_{packageName}"*/), "md", "desc");
            await Verifier.Verify(persistedConfig, settings: settings, extension: "json");
        }
    }

    private static string CreateConfigWithSnippetDocumentationMarkdown(string? snippet)
    {

        return "This plugin has specific configuration stored in a separate configuration file stored in `%APPDATA%/RepoM/Module/` directory. This configuration file should be edit manually. The safest way to do this is, is when RepoM is not running." + Environment.NewLine +
               Environment.NewLine +
               "The following default configuration is used" + Environment.NewLine +
               Environment.NewLine +
               "```json" + Environment.NewLine +
               snippet + Environment.NewLine +
               "```" + Environment.NewLine;
    }

    private static string CreateConfigWithoutSnippetDocumentationMarkdown()
    {
        return "This module has no configuration." + Environment.NewLine;
    }

    private static string CreateConfigSnippet(string content, string snippetName)
    {
        return  $"# begin-snippet: {snippetName}" +
            Environment.NewLine +
            Environment.NewLine +
            content +
            Environment.NewLine +
            Environment.NewLine +
            "# end-snippet";
    }

    private async Task<Tuple<object?, string?>> PersistDefaultConfigAsync(IPackage package)
    {
        Type type = package.GetType();
        MethodInfo? methodInfo = type.GetMethod("PersistDefaultConfigAsync", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);

        if (methodInfo == null)
        {
            return new Tuple<object?, string?>(null, null);
        }

        _fileSystem.RemoveFile("C:\\tmp\\Module\\dummy.json");

        object?[] arguments = { _fileBasedPackageConfiguration, };
        var rawResult = methodInfo.Invoke(package, arguments);

        var persistedContent = _fileSystem.GetFile("C:\\tmp\\Module\\dummy.json").TextContents;
        _fileSystem.RemoveFile("C:\\tmp\\Module\\dummy.json");

        if (rawResult is not Task taskResult)
        {
            return new Tuple<object?, string?>(rawResult, persistedContent);
        }

        await taskResult.ConfigureAwait(false);
        dynamic dynamicTaskResult = taskResult;
        return new Tuple<object?, string?>(dynamicTaskResult.Result, persistedContent);
    }
}