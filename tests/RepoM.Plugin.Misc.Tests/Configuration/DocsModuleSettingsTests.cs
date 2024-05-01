namespace RepoM.Plugin.Misc.Tests.Configuration;

using System;
using System.Collections.Generic;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using NuDoq;
using RepoM.Api.Plugins;
using RepoM.Core.Plugin;
using RepoM.Plugin.Misc.Tests.TestFramework;
using RepoM.Plugin.Misc.Tests.TestFramework.NuDoc;
using VerifyTests;
using VerifyXunit;
using Xunit;

#if !DEBUG
using FluentAssertions;
#endif

public class DocsModuleSettingsTests
{
    private const string VERIFY_DIRECTORY = "ModuleSettingsDocs";
    private readonly VerifySettings _verifySettings = new();
    private readonly FileBasedPackageConfiguration _fileBasedPackageConfiguration;
    private readonly MockFileSystem _fileSystem;

    public DocsModuleSettingsTests()
    {
        _fileSystem = MockFileSystemFactory.CreateDefaultFileSystem();
        _fileBasedPackageConfiguration = new FileBasedPackageConfiguration(MockFileSystemFactory.CreateDefaultAppDataProvider(), _fileSystem, NullLogger.Instance, "dummy");

        _verifySettings.UseDirectory(VERIFY_DIRECTORY);
    }

    public static IEnumerable<object[]> PackagesTestData => PluginStore.Packages.Select(package => new object[] { package, }).ToArray();

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

            var (configExample, _) = await PersistExampleConfigAsync(p);
            if (configExample != null)
            {
                results.Add(p.GetType().Name + "_Example", configExample);
            }
        }

        // assert
        await Verifier.Verify(results, _verifySettings);
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
        _verifySettings.UseTextForParameters(package.GetType().Name);
        if (config == null && persistedConfig == null)
        {
            _verifySettings.AppendContentAsFile(CreateConfigWithoutSnippetDocumentationMarkdown(), "md", "desc");
            await Verifier.Verify($"No config in {packageName}", settings: _verifySettings);
        }
        else
        {
            (_,  string? examplePersistedConfig) = await PersistExampleConfigAsync(package);

            var builtinClassNames = new Dictionary<string, string>
                {
                    [config!.GetType().Name] = "config",
                };

#if DEBUG
            var options = new NuDoq.ReaderOptions
                {
                    KeepNewLinesInText = true,
                };
            AssemblyMembers members = DocReader.Read(config.GetType().Assembly, options);
#else
            var members = new DocumentMembers(System.Xml.Linq.XDocument.Parse("<root></root>"), Array.Empty<Member>());
#endif
            
            var visitor = new PluginConfigurationMarkdownVisitor(builtinClassNames);
            members.Accept(visitor);
            
            var sb = new StringBuilder();
            foreach (ClassWriter classWriter in visitor.ClassWriters.OrderBy(c => c.Key).Select(c => c.Value))
            {
                var head = classWriter.Head.ToString();
                var properties = classWriter.Properties.ToString();

                if (string.IsNullOrWhiteSpace(head) && string.IsNullOrWhiteSpace(properties))
                {
                    continue;
                }

                sb.AppendLine("Properties:");
                sb.AppendLine(string.Empty);
                sb.Append(classWriter.Properties);
            }

            var configWithSnippetDocumentationMarkdown = CreateConfigWithSnippetDocumentationMarkdown(persistedConfig, examplePersistedConfig);

            if (!string.IsNullOrWhiteSpace(sb.ToString()))
            {
                configWithSnippetDocumentationMarkdown += Environment.NewLine + sb;
            }

            _verifySettings.AppendContentAsFile(configWithSnippetDocumentationMarkdown, "md", "desc");

#if DEBUG
            await Verifier.Verify(persistedConfig, settings: _verifySettings, extension: "json");
#else
            true.Should().BeTrue(); // this test should only be run in Debug mode.
#endif
        }
    }
    
    private static string CreateConfigWithSnippetDocumentationMarkdown(string? snippet, string? exampleSnippet = null)
    {
        StringBuilder sb = new StringBuilder()
          .AppendLine("## Configuration")
          .AppendLine(string.Empty)
          .AppendLine("This plugin has specific configuration stored in a separate configuration file stored in `%APPDATA%/RepoM/Module/` directory. This configuration file should be edit manually. The safest way to do this is, is when RepoM is not running.")
          .AppendLine(string.Empty)
          .AppendLine("The following default configuration is used:")
          .AppendLine(string.Empty)
          .AppendLine("```json")
          .AppendLine(snippet)
          .AppendLine("```");

        if (!string.IsNullOrWhiteSpace(exampleSnippet))
        {
            sb.AppendLine(string.Empty)
              .AppendLine("For example:")
              .AppendLine(string.Empty)
              .AppendLine("```json")
              .AppendLine(exampleSnippet)
              .AppendLine("```");
        }

        return sb.ToString();
    }

    private static string CreateConfigWithoutSnippetDocumentationMarkdown()
    {
        return new StringBuilder()
          .AppendLine("## Configuration")
          .AppendLine(string.Empty)
          .AppendLine("This module has no configuration.")
          .ToString();
    }

    private async Task<Tuple<object?, string?>> PersistDefaultConfigAsync(IPackage package)
    {
        MethodInfo? methodInfo = package.GetType().GetMethod("PersistDefaultConfigAsync", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
        return await PersistUsingMethodAsync(package, methodInfo).ConfigureAwait(false);
    }

    private async Task<Tuple<object?, string?>> PersistExampleConfigAsync(IPackage package)
    {
        MethodInfo? methodInfo = package.GetType().GetMethod("PersistExampleConfigAsync", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
        return await PersistUsingMethodAsync(package, methodInfo).ConfigureAwait(false);
    }

    private async Task<Tuple<object?, string?>> PersistUsingMethodAsync(IPackage package, MethodInfo? methodInfo)
    {
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