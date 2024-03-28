namespace RepoM.Plugin.WebBrowser.Tests.ActionMenu.IntegrationTests;

using System.Collections.Generic;
using EasyTestFileXunit;
using System.Threading.Tasks;
using RepoM.ActionMenu.Core.TestLib;
using RepoM.ActionMenu.Interface.UserInterface;
using VerifyXunit;
using Xunit;
using RepoM.Plugin.WebBrowser;

public class WebBrowserBrowserV1Tests : IntegrationActionTestBase<WebBrowserPackage>
{
    [Fact]
    public async Task BrowserScenario01()
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