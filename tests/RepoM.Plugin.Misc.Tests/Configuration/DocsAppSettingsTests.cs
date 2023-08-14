namespace RepoM.Plugin.Misc.Tests.Configuration;

using System.Collections.Generic;
using System.IO.Abstractions.TestingHelpers;
using System.Text;
using System.Threading.Tasks;
using FakeItEasy;
using Microsoft.Extensions.Logging.Abstractions;
using NuDoq;
using RepoM.Api.Common;
using RepoM.Core.Plugin.Common;
using RepoM.Plugin.Misc.Tests.TestFramework.NuDoc;
using VerifyTests;
using VerifyXunit;
using Xunit;

internal static class MockFileSystemFactory
{
    public static MockFileSystem CreateDefaultFileSystem()
    {
        return new MockFileSystem(new Dictionary<string, MockFileData>()
            {
                { "C:\\tmp\\x.tmp", new MockFileData("x") }, // make sure path exists.
            });
    }

    public static IAppDataPathProvider CreateDefaultAppDataProvider()
    {
        IAppDataPathProvider appDataPathProvider = A.Fake<IAppDataPathProvider>();
        A.CallTo(() => appDataPathProvider.AppDataPath).Returns("C:\\tmp\\");
        return appDataPathProvider;
    }
}

[UsesVerify]
public class DocsAppSettingsTests
{
    private FileAppSettingsService _fileBasedPackageConfiguration;
    private readonly MockFileSystem _fileSystem;

    public DocsAppSettingsTests()
    {
        _fileSystem = MockFileSystemFactory.CreateDefaultFileSystem();
        _fileBasedPackageConfiguration = new FileAppSettingsService(MockFileSystemFactory.CreateDefaultAppDataProvider(), _fileSystem, NullLogger.Instance);
    }

    [Fact]
    public async Task AppSettingsJsonFileGeneration()
    {
        // arrange
        var originalValue = _fileBasedPackageConfiguration.PruneOnFetch;

        // act
        // trigger a save.
        _fileBasedPackageConfiguration.PruneOnFetch = !originalValue; // saves
        _fileBasedPackageConfiguration.PruneOnFetch = originalValue; // saves again with default value.

        // assert
        var persistedContent = _fileSystem.GetFile("C:\\tmp\\appsettings.json").TextContents;
        var configWithSnippetDocumentationMarkdown = CreateConfigWithSnippetDocumentationMarkdown(persistedContent);
        
        await Verifier.Verify(configWithSnippetDocumentationMarkdown, extension: "md");
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

    private static string CreateConfigWithSnippetDocumentationMarkdown(string? snippet)
    {
        return new StringBuilder()
           .AppendLine("## Configuration")
           .AppendLine(string.Empty)
           .AppendLine("This is the default configuration.")
           .AppendLine(string.Empty)
           .AppendLine("```json")
           .AppendLine(snippet)
           .AppendLine("```")
           .ToString();
    }
}