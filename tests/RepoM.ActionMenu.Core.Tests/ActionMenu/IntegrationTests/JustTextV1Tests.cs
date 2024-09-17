namespace RepoM.ActionMenu.Core.Tests.ActionMenu.IntegrationTests;

using System.Collections.Generic;
using System.Threading.Tasks;
using EasyTestFileXunit;
using RepoM.ActionMenu.Core.TestLib;
using RepoM.ActionMenu.Interface.UserInterface;
using VerifyXunit;
using Xunit;
using Xunit.Categories;

public class JustTextV1Tests : IntegrationActionTestBase
{
    [Fact]
    [Documentation]
    public async Task DocumentationScenario01()
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