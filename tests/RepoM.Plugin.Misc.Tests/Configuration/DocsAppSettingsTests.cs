namespace RepoM.Plugin.Misc.Tests.Configuration;

using System;
using System.Collections.Generic;
using System.IO.Abstractions.TestingHelpers;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using FakeItEasy;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NuDoq;
using RepoM.Api.Common;
using RepoM.Api.Plugins;
using RepoM.Core.Plugin.Common;
using RepoM.Plugin.Misc.Tests.TestFramework.AssemblyAndTypeHelpers;
using RepoM.Plugin.Misc.Tests.TestFramework.NuDoc;
using VerifyTests;
using VerifyXunit;
using Xunit;

[UsesVerify]
public class DocsAppSettingsTests
{
    private readonly IAppDataPathProvider _appDataPathProvider;
    private FileBasedPackageConfiguration _fileBasedPackageConfiguration;
    private MockFileSystem _fileSystem;
    private ILogger _logger;

    public DocsAppSettingsTests()
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

    [Fact]
    public async Task VerifyChanges()
    {
        // arrange
        var results = new Dictionary<string, Type[]>();

        // act
        foreach (Assembly assembly in PluginStore.Assemblies)
        {
            results.Add(
                assembly.GetName().Name ?? assembly.ToString(),
                assembly.GetRepositoryActionsFromAssembly());
        }

        // assert
        var settings = new VerifySettings();
        await Verifier.Verify(results, settings);
    }

    [Fact]
    public async Task AppSettingsDocumentationGeneration()
    {
        var settings = new VerifySettings();
        settings.UseTextForParameters(nameof(AppSettings));

#if DEBUG
        var options = new NuDoq.ReaderOptions
        {
            KeepNewLinesInText = true,
        };
        AssemblyMembers members = DocReader.Read(typeof(AppSettings).Assembly, options);
#else
        var members = new DocumentMembers(System.Xml.Linq.XDocument.Parse("<root></root>"), Array.Empty<Member>());
#endif

        var visitor = new AppSettingsMarkdownVisitor();
        members.Accept(visitor);

        var sb = new StringBuilder();

        var head = visitor.ClassWriter.Head.ToString();
        var properties = visitor.ClassWriter.Properties.ToString();

        if (!string.IsNullOrWhiteSpace(head) || !string.IsNullOrWhiteSpace(properties))
        {
            sb.AppendLine("Properties:");
            sb.AppendLine(string.Empty);
            sb.Append(visitor.ClassWriter.Properties);
        }

#if DEBUG
        await Verifier.Verify(sb.ToString(), settings: settings, extension: "md");
#else
        await Task.Yield();
        Assert.True(true); // this test should only be run in Debug mode.
#endif
    }
}