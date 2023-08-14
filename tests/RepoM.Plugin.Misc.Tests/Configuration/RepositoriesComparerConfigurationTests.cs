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
using Exception = System.Exception;

public static class TypeExtension
{
    public static string GetTypeValue(this Type type)
    {
        FieldInfo fieldInfo = type.GetField("TYPE_VALUE") ?? throw new Exception("Could not locate field 'TYPE_VALUE'");
        return fieldInfo.GetValue(null)?.ToString() ?? throw new Exception("Could not get value of property 'TYPE_VALUE'");
    }
}

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
        var comparerTypes = RepositoryComparersData
                .Where(x => !x.Assembly.GetName().Name.Contains("Plugin"))
                .OrderBy(x => x.Assembly.GetName().Name)
                .ThenBy(x => x.Type.GetTypeValue());

        const string PREFIX = $"{nameof(RepositoriesComparerConfigurationTests)}.{nameof(DocsRepositoriesComparerConfiguration)}_";
        const string SUFFIX = ".verified.md";

        // act
        foreach (RepositoryTestData item in comparerTypes)
        {
            sb.AppendLine($"### {item.Type.GetTypeValue()}");
            sb.AppendLine(string.Empty);
            sb.AppendLine($"include: {PREFIX}{item.Type.Name}{SUFFIX}"); // mdsnippet
            sb.AppendLine(string.Empty);
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
                sb.AppendLine("This comparer does not have any specific properties.");
            }
            else
            {
                sb.AppendLine("Comparer specific properties:");
                sb.AppendLine(string.Empty);
                sb.Append(classWriter.Properties);
            }
        }

#if DEBUG
        await Verifier.Verify(sb.ToString(), settings: _verifySettings, extension: "md");
#else
        await Task.Yield();
        Assert.True(true); // this test should only be run in Debug mode.
#endif
    }
}