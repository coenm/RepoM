namespace RepoM.Plugin.Misc.Tests.Configuration;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using NuDoq;
using RepoM.Api.IO.ModuleBasedRepositoryActionProvider.Data;
using RepoM.Plugin.Misc.Tests.TestFramework;
using RepoM.Plugin.Misc.Tests.TestFramework.AssemblyAndTypeHelpers;
using RepoM.Plugin.Misc.Tests.TestFramework.NuDoc;
using VerifyTests;
using VerifyXunit;
using Xunit;

public class DocsRepositoryActionsTests
{
    private const string VERIFY_DIRECTORY = "RepositoryActionsDocs";
    private readonly VerifySettings _verifySettings = new();

    public DocsRepositoryActionsTests()
    {
        _verifySettings.UseDirectory(VERIFY_DIRECTORY);
        _verifySettings.IncludeObsoletes();
    }

    public static IEnumerable<object[]> AssemblyTestData => PluginStore.Assemblies.Select(assembly => new object[] { assembly, }).ToArray();

    public static IEnumerable<object[]> RepositoryActionsTestData
    {
        get
        {
            List<object[]> results = new();

            foreach (Assembly assembly in RepoMAssemblyStore.GetAssemblies())
            {
                try
                {
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
        var assemblies = RepoMAssemblyStore.GetAssemblies()
                                           .Concat(PluginStore.Assemblies)
                                           .Distinct()
                                           .OrderBy(a => a.FullName);

        // act
        var results = assemblies.ToDictionary(
            assembly => assembly.GetName().Name ?? assembly.ToString(),
            assembly => assembly.GetRepositoryActionsFromAssembly());

        // assert
        await Verifier.Verify(results, _verifySettings).IncludeObsoletes();
    }

    [Fact]
    public async Task RepositoryActionBaseDocumentationGeneration()
    {
        _verifySettings.UseTextForParameters(nameof(RepositoryAction));

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

        var head = visitor.ClassWriter.Head.ToString();
        var properties = visitor.ClassWriter.Properties.ToString();

        if (!string.IsNullOrWhiteSpace(head) || !string.IsNullOrWhiteSpace(properties))
        {
            sb.AppendLine("Properties:");
            sb.AppendLine(string.Empty);
            sb.Append(visitor.ClassWriter.Properties);
        }

#if DEBUG
        await Verifier.Verify(sb.ToString(), settings: _verifySettings, extension: "md");
#else
        await Task.Yield();
        true.Should().BeTrue(); // this test should only be run in Debug mode.
#endif
    }

//     [Theory]
//     [MemberData(nameof(RepositoryActionsTestData))]
//     public async Task DocsRepositoryActionsSettings(RepositoryTestData repositoryActionTestData)
//     {
//         _verifySettings.UseTextForParameters(repositoryActionTestData.Type.Name);
//
//         var builtinClassNames = new Dictionary<string, string>
//             {
//                 [repositoryActionTestData.Type.Name] = "config",
//             };
//
// #if DEBUG
//         var options = new NuDoq.ReaderOptions
//             {
//                 KeepNewLinesInText = true,
//             };
//         AssemblyMembers members = DocReader.Read(repositoryActionTestData.Assembly, options);
// #else
//         var members = new DocumentMembers(System.Xml.Linq.XDocument.Parse("<root></root>"), Array.Empty<Member>());
// #endif
//
//         var visitor = new RepositoryActionMarkdownVisitor(builtinClassNames);
//         members.Accept(visitor);
//
//         var sb = new StringBuilder();
//         foreach (ClassWriter classWriter in visitor.ClassWriters.OrderBy(c => c.Key).Select(c => c.Value))
//         {
//             var head = classWriter.Head.ToString();
//             var properties = classWriter.Properties.ToString();
//
//             head = head.Trim();
//             sb.AppendLine(head);
//             sb.AppendLine(string.Empty);
//
//             if (string.IsNullOrWhiteSpace(properties))
//             {
//                 sb.AppendLine("This action does not have any specific properties.");
//             }
//             else
//             {
//                 sb.AppendLine("Action specific properties:");
//                 sb.AppendLine(string.Empty);
//                 sb.Append(classWriter.Properties);
//             }
//         }
//
// #if DEBUG
//         await Verifier.Verify(sb.ToString(), settings: _verifySettings, extension: "md");
// #else
//         await Task.Yield();
//         true.Should().BeTrue(); // this test should only be run in Debug mode.
// #endif
//     }
}