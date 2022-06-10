namespace RepoM.Api.Common.Tests.IO.ModuleBasedRepositoryActionProvider.Action;

using System.Threading.Tasks;
using EasyTestFile;
using EasyTestFileXunit;
using FluentAssertions;
using RepoM.Api.Common.IO.ModuleBasedRepositoryActionProvider;
using RepoM.Api.Common.IO.ModuleBasedRepositoryActionProvider.ActionDeserializers;
using RepoM.Api.Common.IO.ModuleBasedRepositoryActionProvider.Data.Actions;
using VerifyTests;
using VerifyXunit;
using Xunit;

[UsesEasyTestFile]
[UsesVerify]
public class GitCheckoutV1Test
{
    private readonly DynamicRepositoryActionDeserializer _sut;
    private readonly EasyTestFileSettings _testFileSettings;
    private readonly VerifySettings _verifySettings;

    public GitCheckoutV1Test()
    {
        _sut = DynamicRepositoryActionDeserializerFactory.CreateWithDeserializer(new ActionGitCheckoutV1Deserializer());

        _testFileSettings = new EasyTestFileSettings();
        _testFileSettings.UseDirectory("TestFiles");
        _testFileSettings.UseExtension("json");

        _verifySettings = new VerifySettings();
        _verifySettings.UseDirectory("Verified");
    }

    [Fact]
    public async Task Deserialize_GitCheckoutV1()
    {
        // arrange
        var content = await EasyTestFile.LoadAsText(_testFileSettings);

        // act
        var result = _sut.Deserialize(content);

        // assert
        await Verifier.Verify(result, _verifySettings);
    }

    [Fact]
    public async Task Deserialize_ShouldBeOfExpectedType()
    {
        // arrange
        _testFileSettings.UseMethodName(nameof(Deserialize_GitCheckoutV1));
        var content = await EasyTestFile.LoadAsText(_testFileSettings);

        // act
        var result = _sut.Deserialize(content);

        // assert
        _ = result.ActionsCollection.Actions.Should().AllBeOfType<RepositoryActionGitCheckoutV1>();
    }
}