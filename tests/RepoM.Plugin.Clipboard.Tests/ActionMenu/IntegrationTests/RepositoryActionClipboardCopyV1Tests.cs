namespace RepoM.Plugin.Clipboard.Tests.ActionMenu.IntegrationTests;

using System.Collections.Generic;
using EasyTestFileXunit;
using System.Threading.Tasks;
using RepoM.ActionMenu.Core.TestLib;
using RepoM.ActionMenu.Interface.UserInterface;
using RepoM.Plugin.Clipboard;
using VerifyXunit;
using Xunit;

public class RepositoryActionClipboardCopyV1Tests : IntegrationActionTestBase<ClipboardPackage>
{
    [Fact]
    public async Task ClipboardCopyScenario01()
    {
        // arrange
        var content = await EasyTestFile.LoadAsText(TestFileSettings);
        AddRootFile(content);

        // act
        IEnumerable<UserInterfaceRepositoryActionBase> result = await CreateMenuAsync();

        // assert
        await Verifier.Verify(result, VerifySettings);
    }
}