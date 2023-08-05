namespace RepoM.Plugin.Misc.Tests.Configuration;

using System;
using System.Collections.Generic;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using FakeItEasy;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NuDoq;
using RepoM.Api.IO.ModuleBasedRepositoryActionProvider.Data;
using RepoM.Api.Plugins;
using RepoM.Core.Plugin.Common;
using RepoM.Plugin.Misc.Tests.TestFramework.AssemblyAndTypeHelpers;
using RepoM.Plugin.Misc.Tests.TestFramework.NuDoc;
using VerifyTests;
using VerifyXunit;
using Xunit;

[UsesVerify]
public class DocsRepositoryActionsTests
{
    private readonly IAppDataPathProvider _appDataPathProvider;
    private FileBasedPackageConfiguration _fileBasedPackageConfiguration;
    private MockFileSystem _fileSystem;
    private ILogger _logger;

    public DocsRepositoryActionsTests()
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

    public static IEnumerable<object[]> AssemblyTestData => PluginStore.Assemblies.Select(assembly => new object[] { assembly, }).ToArray();

    public static IEnumerable<object[]> RepositoryActionsTestData
    {
        get
        {
            List<object[]> results = new();

            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    // Workaround for Github Actions
                    if (assembly.GetName().Name.Equals("RepoM.Plugin.Misc.Tests"))
                    {
                        continue;
                    }

                    foreach (Type repositoryActionType in assembly.GetRepositoryActionsFromAssembly())
                    {
                        results.Add(new object[] { new RepositoryTestData(assembly, repositoryActionType), });
                    }
                }
                catch (System.Exception)
                {
                    // skip
                }
            }

            return results;
        }
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

    public class RepositoryTestData
    {
        public RepositoryTestData(Assembly assembly, Type type)
        {
            Assembly = assembly;
            Type = type;
        }

        public Assembly Assembly { get; }

        public Type Type { get; }

        public override string ToString()
        {
            return Assembly.GetName().Name + "-" + Type.Name;
        }
    }

    [Fact]
    public async Task RepositoryActionBaseDocumentationGeneration()
    {
        var settings = new VerifySettings();
        // settings.AutoVerify();
        settings.UseDirectory("VerifiedDocs1");
        settings.UseTextForParameters(nameof(RepositoryAction));

#if DEBUG
        var options = new NuDoq.ReaderOptions
        {
            KeepNewLinesInText = true,
        };
        AssemblyMembers members = DocReader.Read(typeof(RepositoryAction).Assembly, options);
#else
        var members = new DocumentMembers(System.Xml.Linq.XDocument.Parse("<root></root>"), Array.Empty<Member>());
#endif

        var visitor = new RepositoryActionBaseMarkdownVisitor(typeof(RepositoryAction));
        members.Accept(visitor);

        var sb = new StringBuilder();

        var head = visitor.classWriter.Head.ToString();
        var properties = visitor.classWriter.Properties.ToString();

        if (!string.IsNullOrWhiteSpace(head) || !string.IsNullOrWhiteSpace(properties))
        {
            sb.AppendLine("Properties:");
            sb.AppendLine(string.Empty);
            sb.Append(visitor.classWriter.Properties);
        }

#if DEBUG
        await Verifier.Verify(sb.ToString(), settings: settings, extension: "md");
#else
        await Task.Yield();
        Assert.True(true); // this test should only be run in Debug mode.
#endif
    }

    [Theory]
    [MemberData(nameof(RepositoryActionsTestData))]
    public async Task DocsRepositoryActionsSettings(RepositoryTestData repositoryActionTestData)
    {
        var settings = new VerifySettings();
        // settings.AutoVerify();
        settings.UseDirectory("VerifiedDocs1");
        settings.UseTextForParameters(repositoryActionTestData.Type.Name);

        var builtinClassNames = new Dictionary<string, string>
            {
                [repositoryActionTestData.Type.Name] = "config",
            };

#if DEBUG
        var options = new NuDoq.ReaderOptions
            {
                KeepNewLinesInText = true,
            };
        AssemblyMembers members = DocReader.Read(repositoryActionTestData.Assembly, options);
#else
        var members = new DocumentMembers(System.Xml.Linq.XDocument.Parse("<root></root>"), Array.Empty<Member>());
#endif

        var visitor = new RepositoryActionMarkdownVisitor(builtinClassNames);
        members.Accept(visitor);

        var sb = new StringBuilder();
        foreach (ClassWriter classWriter in visitor.ClassWriters.OrderBy(c => c.Key).Select(c => c.Value))
        {
            var head = classWriter.Head.ToString();
            var properties = classWriter.Properties.ToString();

            head = head.Trim();
            sb.AppendLine(head);
            sb.AppendLine(string.Empty);

            if (string.IsNullOrWhiteSpace(properties))
            {
                sb.AppendLine("This action does not have any specific properties.");
            }
            else
            {
                sb.AppendLine("Action specific properties:");
                sb.AppendLine(string.Empty);
                sb.Append(classWriter.Properties);
            }
        }

#if DEBUG
        await Verifier.Verify(sb.ToString(), settings: settings, extension: "md");
#else
        await Task.Yield();
        Assert.True(true); // this test should only be run in Debug mode.
#endif
    }
}