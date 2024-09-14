namespace RepoM.Plugin.Misc.Tests.Configuration;

using System;
using System.Collections.Generic;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using RepoM.Api.Plugins;
using RepoM.Core.Plugin;
using RepoM.Plugin.Misc.Tests.TestFramework;
using VerifyTests;
using VerifyXunit;
using Xunit;

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

        object?[] arguments = [_fileBasedPackageConfiguration,];
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