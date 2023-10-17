namespace RepoM.Plugin.Misc.Tests.Configuration;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using NuDoq;
using RepoM.Plugin.Misc.Tests.TestFramework;
using RepoM.Plugin.Misc.Tests.TestFramework.AssemblyAndTypeHelpers;
using RepoM.Plugin.Misc.Tests.TestFramework.NuDoc;
using VerifyTests;
using VerifyXunit;
using Xunit;

[UsesVerify]
public class RepositoriesComparerConfigurationTests
{
    private const string VERIFY_DIRECTORY = "RepositoriesComparerConfigurationDocs";
    private readonly VerifySettings _verifySettings = new();

    public RepositoriesComparerConfigurationTests()
    {
        _verifySettings.UseDirectory(VERIFY_DIRECTORY);
    }

    public static IEnumerable<object[]> PackagesTestData => PluginStore.Packages.Select(package => new object[] { package, }).ToArray();

    public static IEnumerable<RepositoryTestData> RepositoryComparersData
    {
        get
        {
            List<RepositoryTestData> results = new();

            foreach (Assembly assembly in RepoMAssemblyStore.GetAssemblies())
            {
                try
                {
                    foreach (Type repositoryActionType in assembly.GetRepositoriesComparerConfigurationFromAssembly())
                    {
                        results.Add(new RepositoryTestData(assembly, repositoryActionType));
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

    public static IEnumerable<object[]> RepositoryComparersDataXunit
    {
        get
        {
            return RepositoryComparersData.Select(x => new object[] { x, }).ToArray();
        }
    }

    [Fact]
    public async Task VerifyChanges()
    {
        // arrange
        var assemblies = RepoMAssemblyStore.GetAssemblies()
                                           .Concat(PluginStore.Assemblies)
                                           .Distinct()
                                           .OrderBy(a => a.FullName);

        // act
        var results = assemblies.ToDictionary(
            assembly => assembly.GetName().Name ?? assembly.ToString(),
            assembly => assembly.GetRepositoriesComparerConfigurationFromAssembly());

        // assert
        await Verifier.Verify(results, _verifySettings);
    }

    [Fact]
    public async Task CoreComparersMarkdown()
    {
        // arrange
        var sb = new StringBuilder();
        var coreComparerTypes = RepositoryComparersData
                .Where(x => !x.Assembly.IsRepoMPluginAssembly())
                .OrderBy(x => x.Assembly.GetName().Name)
                .ThenBy(x => x.Type.GetTypeValue())
                .ToArray();
        var pluginComparerTypes = RepositoryComparersData
                .Where(x => x.Assembly.IsRepoMPluginAssembly())
                .OrderBy(x => x.Assembly.GetName().Name)
                .ThenBy(x => x.Type.GetTypeValue())
                .ToArray();

        const string PREFIX = $"{nameof(RepositoriesComparerConfigurationTests)}.{nameof(DocsRepositoriesComparerConfiguration)}_";
        const string SUFFIX = ".verified.md";

        static string CreateTableOfContentItem(Type type)
        {
            return $"- [`{type.GetTypeValue()}`](#{MarkdownHelpers.CreateGithubMarkdownAnchor(type.GetTypeValue())})";
        }

        static void AppendToStringBuilder(StringBuilder sb, Type t)
        {
            sb.AppendLine($"### `{t.GetTypeValue()}`");
            sb.AppendLine(string.Empty);
            sb.AppendLine($"include: {PREFIX}{t.Name}{SUFFIX}"); // mdsnippet
            sb.AppendLine(string.Empty);
        }

        // act

        foreach (RepositoryTestData item in coreComparerTypes)
        {
            sb.AppendLine(CreateTableOfContentItem(item.Type));
        }

        if (pluginComparerTypes.Length > 0)
        {
            sb.AppendLine(string.Empty);
            sb.AppendLine("These comparers are available by using the corresponding plugin.");
            foreach (RepositoryTestData item in pluginComparerTypes)
            {
                sb.AppendLine(CreateTableOfContentItem(item.Type));
            }
        }

        sb.AppendLine(string.Empty);

        foreach (RepositoryTestData item in coreComparerTypes)
        {
            AppendToStringBuilder(sb, item.Type);
        }

        foreach (RepositoryTestData item in pluginComparerTypes)
        {
            AppendToStringBuilder(sb, item.Type);
        }

        // assert
        await Verifier.Verify(sb.ToString(), settings: _verifySettings, extension: "md");
    }

    [Theory]
    [MemberData(nameof(RepositoryComparersDataXunit))]
    public async Task DocsRepositoriesComparerConfiguration(RepositoryTestData repositoryActionTestData)
    {
        _verifySettings.UseTextForParameters(repositoryActionTestData.Type.Name);

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

        var visitor = new RepositoryComparerSettingMarkdownVisitor(builtinClassNames);
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
                sb.AppendLine("This comparer does not have properties.");
            }
            else
            {
                sb.AppendLine("Properties:");
                sb.AppendLine(string.Empty);
                sb.Append(classWriter.Properties);
            }
        }

#if DEBUG
        await Verifier.Verify(sb.ToString(), settings: _verifySettings, extension: "md");
#else
        await Task.Yield();
        true.Should().BeTrue(); // this test should only be run in Debug mode.
#endif
    }
}