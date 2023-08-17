namespace RepoM.Plugin.AzureDevOps.Tests.ActionProvider;

using System;
using System.Threading.Tasks;
using EasyTestFile;
using EasyTestFileXunit;
using FluentAssertions;
using RepoM.Api.IO.ModuleBasedRepositoryActionProvider;
using RepoM.Api.IO.ModuleBasedRepositoryActionProvider.ActionDeserializers;
using RepoM.Api.IO.ModuleBasedRepositoryActionProvider.Data;
using RepoM.Api.IO.ModuleBasedRepositoryActionProvider.Deserialization;
using RepoM.Core.Plugin.RepositoryOrdering.Configuration;
using RepoM.Plugin.AzureDevOps.ActionProvider.Options;
using VerifyTests;
using VerifyXunit;
using Xunit;

[UsesEasyTestFile]
[UsesVerify]
public class AzureDevOpsGetPullRequestsV1Test
{
    private readonly EasyTestFileSettings _testFileSettings;
    private readonly VerifySettings _verifySettings;
    private readonly YamlDynamicRepositoryActionDeserializer _sut;

    public AzureDevOpsGetPullRequestsV1Test()
    {
        _sut = new YamlDynamicRepositoryActionDeserializer(CreateWithDeserializer(new DefaultActionDeserializer<RepositoryActionAzureDevOpsGetPullRequestsV1>()));

        _testFileSettings = new EasyTestFileSettings();
        _testFileSettings.UseDirectory("TestFiles");
        _testFileSettings.UseExtension("yaml");

        _verifySettings = new VerifySettings();
        _verifySettings.UseDirectory("Verified");
    }

    [Fact]
    public async Task Deserialize()
    {
        // arrange
        var content = await EasyTestFile.LoadAsText(_testFileSettings);

        // act
        RepositoryActionConfiguration result = SutDeserialize(content);

        // assert
        await Verifier.Verify(result, _verifySettings);
    }

    [Fact]
    public async Task Deserialize_ShouldBeOfExpectedType()
    {
        // arrange
        _testFileSettings.UseMethodName(nameof(Deserialize));
        var content = await EasyTestFile.LoadAsText(_testFileSettings);

        // act
        RepositoryActionConfiguration result = SutDeserialize(content);

        // assert
        _ = result.ActionsCollection.Actions.Should().AllBeOfType<RepositoryActionAzureDevOpsGetPullRequestsV1>();
    }

    private RepositoryActionConfiguration SutDeserialize(string rawContent)
    {
        return _sut.Deserialize(rawContent);
    }

    private static ActionDeserializerComposition CreateWithDeserializer(IActionDeserializer actionDeserializer)
    {
        return new ActionDeserializerComposition(new IActionDeserializer[] { actionDeserializer, }, Array.Empty<IKeyTypeRegistration<RepositoryAction>>());
    }
}