namespace RepoM.Plugin.WebBrowser.Tests.ActionProvider;

using System;
using System.Threading.Tasks;
using EasyTestFile;
using EasyTestFileXunit;
using FluentAssertions;
using RepoM.Api.IO.ModuleBasedRepositoryActionProvider.ActionDeserializers;
using RepoM.Api.IO.ModuleBasedRepositoryActionProvider.Data;
using RepoM.Api.IO.ModuleBasedRepositoryActionProvider.Deserialization;
using RepoM.Core.Plugin.RepositoryOrdering.Configuration;
using RepoM.Plugin.WebBrowser.ActionProvider;
using VerifyTests;
using VerifyXunit;
using Xunit;

[UsesEasyTestFile]
[UsesVerify]
public class BrowserV1Test
{
    private readonly YamlDynamicRepositoryActionDeserializer _sut;
    private readonly EasyTestFileSettings _testFileSettings;
    private readonly VerifySettings _verifySettings;

    public BrowserV1Test()
    {
        var actionDeserializerComposition = new ActionDeserializerComposition(new[] { new DefaultActionDeserializer<RepositoryActionBrowserV1>(), }, Array.Empty<IKeyTypeRegistration<RepositoryAction>>());
        _sut = new YamlDynamicRepositoryActionDeserializer(actionDeserializerComposition);

        _testFileSettings = new EasyTestFileSettings();
        _testFileSettings.UseDirectory("TestFiles");
        _testFileSettings.UseExtension("yaml");

        _verifySettings = new VerifySettings();
        _verifySettings.UseDirectory("Verified");
    }

    [Fact]
    public async Task Deserialize_ActionBrowserV1()
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
        _testFileSettings.UseMethodName(nameof(Deserialize_ActionBrowserV1));
        var content = await EasyTestFile.LoadAsText(_testFileSettings);

        // act
        var result = _sut.Deserialize(content);

        // assert
        _ = result.ActionsCollection.Actions.Should().AllBeOfType<RepositoryActionBrowserV1>();
    }
}