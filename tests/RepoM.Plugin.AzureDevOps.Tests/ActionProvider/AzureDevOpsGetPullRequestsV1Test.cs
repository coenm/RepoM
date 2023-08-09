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
using RepoM.Api.Tests.IO.ModuleBasedRepositoryActionProvider;
using RepoM.Plugin.AzureDevOps.ActionProvider.Options;
using VerifyTests;
using VerifyXunit;
using Xunit;
using XunitEnumMemberData;

[UsesEasyTestFile]
[UsesVerify]
public class AzureDevOpsGetPullRequestsV1Test
{
    private readonly JsonDynamicRepositoryActionDeserializer _sutJson;
    private readonly EasyTestFileSettings _testFileSettings;
    private readonly VerifySettings _verifySettings;
    private readonly YamlDynamicRepositoryActionDeserializer _sutYaml;

    public AzureDevOpsGetPullRequestsV1Test()
    {
        _sutJson = CreateWithDeserializer(new DefaultActionDeserializer<RepositoryActionAzureDevOpsGetPullRequestsV1>());
        _sutYaml = new YamlDynamicRepositoryActionDeserializer(_sutJson);

        _testFileSettings = new EasyTestFileSettings();
        _testFileSettings.UseDirectory("TestFiles");
        _testFileSettings.UseExtension("json");

        _verifySettings = new VerifySettings();
        _verifySettings.UseDirectory("Verified");
    }

    [Theory]
    [EnumMemberData(typeof(SerializationType))]
    public async Task Deserialize(SerializationType type)
    {
        // arrange
        var content = await EasyTestFile.LoadAsText(_testFileSettings.SetExtension(type));

        // act
        RepositoryActionConfiguration result = SutDeserialize(content, type);

        // assert
        await Verifier.Verify(result, _verifySettings).IgnoreParametersForVerified(type);
    }

    [Fact]
    public async Task Deserialize_ShouldBeOfExpectedType()
    {
        // arrange
        _testFileSettings.UseMethodName(nameof(Deserialize));
        var content = await EasyTestFile.LoadAsText(_testFileSettings);

        // act
        RepositoryActionConfiguration result = SutDeserialize(content, SerializationType.Json);

        // assert
        _ = result.ActionsCollection.Actions.Should().AllBeOfType<RepositoryActionAzureDevOpsGetPullRequestsV1>();
    }

    private RepositoryActionConfiguration SutDeserialize(string rawContent, SerializationType type)
    {
        return type switch
        {
            SerializationType.Json => _sutJson.Deserialize(rawContent),
            SerializationType.Yaml => _sutYaml.Deserialize(rawContent),
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null),
        };
    }

    private static JsonDynamicRepositoryActionDeserializer CreateWithDeserializer(IActionDeserializer actionDeserializer)
    {
        return new JsonDynamicRepositoryActionDeserializer(new ActionDeserializerComposition(new IActionDeserializer[] { actionDeserializer, }));
    }
}