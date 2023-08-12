namespace RepoM.Api.Tests.IO.ModuleBasedRepositoryActionProvider;

using System;
using System.Threading.Tasks;
using EasyTestFile;
using EasyTestFileXunit;
using FluentAssertions;
using Newtonsoft.Json;
using RepoM.Api.IO.ModuleBasedRepositoryActionProvider.Data;
using RepoM.Api.IO.ModuleBasedRepositoryActionProvider.Deserialization;
using VerifyTests;
using VerifyXunit;
using Xunit;
using Xunit.Abstractions;

[UsesEasyTestFile]
[UsesVerify]
public class DynamicRepositoryActionDeserializerTest
{
    private readonly ITestOutputHelper _outputHelper;
    private readonly JsonDynamicRepositoryActionDeserializer _sutJson;
    private readonly EasyTestFileSettings _testFileSettings;
    private readonly VerifySettings _verifySettings;
    private readonly YamlDynamicRepositoryActionDeserializer _sutYaml;

    public DynamicRepositoryActionDeserializerTest(ITestOutputHelper outputHelper)
    {
        _outputHelper = outputHelper ?? throw new ArgumentNullException(nameof(outputHelper));
        _sutJson = DynamicRepositoryActionDeserializerFactory.Create();
        _sutYaml = new YamlDynamicRepositoryActionDeserializer(_sutJson);

        _testFileSettings = new EasyTestFileSettings();
        _testFileSettings.UseDirectory("TestFiles");
        _testFileSettings.UseExtension("json");

        _verifySettings = new VerifySettings();
        _verifySettings.UseDirectory("Verified");
    }

    private RepositoryActionConfiguration SutDeserialize(string rawContent, SerializationType type)
    {
        return type switch
            {
                SerializationType.Yaml => _sutYaml.Deserialize(rawContent),
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, null),
            };
    }

    [Fact]
    public async Task Deserialize_ShouldReturnData_WhenContentIsEmptyVersion1()
    {
        // arrange
        SerializationType type = SerializationType.Yaml;
        _testFileSettings.UseFileName("Version1");
        var content = await EasyTestFile.LoadAsText(_testFileSettings.SetExtension(type));

        // act
        RepositoryActionConfiguration result = SutDeserialize(content, type);

        // assert
        await Verifier.Verify(result, _verifySettings).IgnoreParametersForVerified(type);
    }
    
    [Fact]
    public async Task Deserialize_RepositorySpecificEnvFiles_1()
    {
        // arrange
        SerializationType type = SerializationType.Yaml;
        _testFileSettings.UseFileName("RepositorySpecificEnvFilesExplanation1");
        var content = await EasyTestFile.LoadAsText(_testFileSettings.SetExtension(type));

        // act
        RepositoryActionConfiguration result = SutDeserialize(content, type);

        // assert
        await Verifier.Verify(result, _verifySettings).IgnoreParametersForVerified(type);
    }

    [Fact]
    public async Task Deserialize_RepositoryTagsFiles_1()
    {
        // arrange
        SerializationType type = SerializationType.Yaml;
        _testFileSettings.UseFileName("RepositoryTagsExplanation1");
        var content = await EasyTestFile.LoadAsText(_testFileSettings.SetExtension(type));

        // act
        RepositoryActionConfiguration result = SutDeserialize(content, type);

        // assert
        await Verifier.Verify(result, _verifySettings).IgnoreParametersForVerified(type);
    }

    [Fact]
    public async Task Deserialize_ShouldReturnEmptyObject_WhenContentIsEmpty()
    {
        // arrange
        SerializationType type = SerializationType.Yaml;
        _testFileSettings.UseFileName("Empty");
        var content = await EasyTestFile.LoadAsText(_testFileSettings.SetExtension(type));

        // act
        RepositoryActionConfiguration result = SutDeserialize(content, type);

        // assert
        await Verifier.Verify(result, _verifySettings).IgnoreParametersForVerified(type);
    }

    [Fact]
    public async Task Deserialize_ShouldReturnEmptyObject_WhenVersionIsUnknown()
    {
        // arrange
        SerializationType type = SerializationType.Yaml;
        _testFileSettings.UseFileName("Version100");
        var content = await EasyTestFile.LoadAsText(_testFileSettings.SetExtension(type));

        // act
        RepositoryActionConfiguration result = SutDeserialize(content, type);

        // assert
        await Verifier.Verify(result, _verifySettings).IgnoreParametersForVerified(type);
    }

    [Fact]
    public async Task Deserialize_ShouldReturnObjectWithVariables_WhenContentIsVariablesOnly()
    {
        // arrange
        SerializationType type = SerializationType.Yaml;
        _testFileSettings.UseFileName("VariablesOnly1");
        var content = await EasyTestFile.LoadAsText(_testFileSettings.SetExtension(type));

        // act
        RepositoryActionConfiguration result = SutDeserialize(content, type);

        // assert
        await Verifier.Verify(result, _verifySettings).IgnoreParametersForVerified(type);
    }

    [Fact]
    public async Task Deserialize_ShouldReturnObjectWithRepositoryTags_WhenContentIsRepositoryTags1()
    {
        // arrange
        SerializationType type = SerializationType.Yaml;
        _testFileSettings.UseFileName("RepositoryTags1");
        var content = await EasyTestFile.LoadAsText(_testFileSettings.SetExtension(type));

        // act
        RepositoryActionConfiguration result = SutDeserialize(content, type);

        // assert
        await Verifier.Verify(result, _verifySettings).IgnoreParametersForVerified(type);
    }

    [Fact]
    public async Task Deserialize_ShouldReturnObjectWithLatestTags_WhenContentHasDoubleTags()
    {
        // arrange
        SerializationType type = SerializationType.Yaml;
        _testFileSettings.UseFileName("RepositoryTagsDouble");
        var content = await EasyTestFile.LoadAsText(_testFileSettings.SetExtension(type));

        // act
        RepositoryActionConfiguration result = SutDeserialize(content, type);

        // assert
        await Verifier.Verify(result, _verifySettings).IgnoreParametersForVerified(type);
    }

    [Fact]
    public async Task Deserialize_ShouldReturnObjectWithRepositoryTags_WhenContentIsRepositoryTags2()
    {
        // arrange
        SerializationType type = SerializationType.Yaml;
        _testFileSettings.UseFileName("RepositoryTags2");
        var content = await EasyTestFile.LoadAsText(_testFileSettings.SetExtension(type));

        // act
        RepositoryActionConfiguration result = SutDeserialize(content, type);

        // assert
        await Verifier.Verify(result, _verifySettings)
                      .UseMethodName(nameof(Deserialize_ShouldReturnObjectWithRepositoryTags_WhenContentIsRepositoryTags1))
                      .IgnoreParametersForVerified(type);
    }

    [Fact]
    public async Task Deserialize_ShouldReturnObjectWithRepositoryTags_WhenContentIsRepositoryTags3()
    {
        // arrange
        SerializationType type = SerializationType.Yaml;
        _testFileSettings.UseFileName("RepositoryTags3");
        var content = await EasyTestFile.LoadAsText(_testFileSettings.SetExtension(type));

        // act
        RepositoryActionConfiguration result = SutDeserialize(content, type);

        // assert
        await Verifier.Verify(result, _verifySettings).IgnoreParametersForVerified(type);
    }

    [Fact]
    public async Task Deserialize_ShouldReturnObjectWithRepositoryActions_WhenContentIsRepositoryActions1()
    {
        // arrange
        SerializationType type = SerializationType.Yaml;
        _testFileSettings.UseFileName("RepositoryActions1");
        var content = await EasyTestFile.LoadAsText(_testFileSettings.SetExtension(type));

        // act
        RepositoryActionConfiguration result = SutDeserialize(content, type);

        // assert
        await Verifier.Verify(result, _verifySettings).IgnoreParametersForVerified(type);
    }

    [Fact]
    public async Task Deserialize_ShouldReturnObjectWithRepositoryActions_WhenContentIsRepositoryActions2()
    {
        // arrange
        SerializationType type = SerializationType.Yaml;
        _testFileSettings.UseFileName("RepositoryActions2");
        var content = await EasyTestFile.LoadAsText(_testFileSettings.SetExtension(type));

        // act
        RepositoryActionConfiguration result = SutDeserialize(content, type);

        // assert
        await Verifier.Verify(result, _verifySettings)
                      .IgnoreParametersForVerified(type)
                      .UseMethodName(nameof(Deserialize_ShouldReturnObjectWithRepositoryActions_WhenContentIsRepositoryActions1));
    }

    [Fact]
    public async Task Deserialize_ShouldReturnObjectWithRepositoryActions_WhenContentIsRepositoryActions3()
    {
        // arrange
        SerializationType type = SerializationType.Yaml;
        _testFileSettings.UseFileName("RepositoryActions3");
        var content = await EasyTestFile.LoadAsText(_testFileSettings.SetExtension(type));

        // act
        RepositoryActionConfiguration result = SutDeserialize(content, type);

        // assert
        await Verifier.Verify(result, _verifySettings).IgnoreParametersForVerified(type);
    }

    [Fact]
    public async Task Deserialize_ShouldReturnObjectWithRedirect_WhenContentIsRedirect1()
    {
        // arrange
        SerializationType type = SerializationType.Yaml;
        _testFileSettings.UseFileName("Redirect1");
        var content = await EasyTestFile.LoadAsText(_testFileSettings.SetExtension(type));

        // act
        RepositoryActionConfiguration result = SutDeserialize(content, type);

        // assert
        await Verifier.Verify(result, _verifySettings).IgnoreParametersForVerified(type);
    }

    [Fact]
    public async Task Deserialize_ShouldReturnObjectWithRedirect_WhenContentIsRedirect2()
    {
        // arrange
        SerializationType type = SerializationType.Yaml;
        _testFileSettings.UseFileName("Redirect2");
        var content = await EasyTestFile.LoadAsText(_testFileSettings.SetExtension(type));

        // act
        RepositoryActionConfiguration result = SutDeserialize(content, type);

        // assert
        await Verifier.Verify(result, _verifySettings)
                      .IgnoreParametersForVerified(type)
                      .UseMethodName(nameof(Deserialize_ShouldReturnObjectWithRedirect_WhenContentIsRedirect1));
    }

    [Fact]
    public async Task Deserialize_ShouldReturnObjectWithRedirect_WhenContentIsRedirect3()
    {
        // arrange
        SerializationType type = SerializationType.Yaml;
        _testFileSettings.UseFileName("Redirect3");
        var content = await EasyTestFile.LoadAsText(_testFileSettings.SetExtension(type));

        // act
        RepositoryActionConfiguration result = SutDeserialize(content, type);

        // assert
        await Verifier.Verify(result, _verifySettings).IgnoreParametersForVerified(type);
    }

    [Fact]
    public async Task Deserialize_ShouldReturnObject_WhenContentIsRepositorySpecificEnvFile1()
    {
        // arrange
        SerializationType type = SerializationType.Yaml;
        _testFileSettings.UseFileName("RepositorySpecificEnvFile1");
        var content = await EasyTestFile.LoadAsText(_testFileSettings.SetExtension(type));

        // act
        RepositoryActionConfiguration result = SutDeserialize(content, type);

        // assert
        await Verifier.Verify(result, _verifySettings).IgnoreParametersForVerified(type);
    }

    [Fact]
    public async Task Deserialize_ShouldReturnObject_WhenContentIsRepositorySpecificConfigFile1()
    {
        // arrange
        SerializationType type = SerializationType.Yaml;
        _testFileSettings.UseFileName("RepositorySpecificConfigFile1");
        var content = await EasyTestFile.LoadAsText(_testFileSettings.SetExtension(type));

        // act
        RepositoryActionConfiguration result = SutDeserialize(content, type);

        // assert
        await Verifier.Verify(result, _verifySettings).IgnoreParametersForVerified(type);
    }

    [Fact]
    public async Task Deserialize_Sample1()
    {
        // arrange
        SerializationType type = SerializationType.Yaml;
        _testFileSettings.UseFileName("Sample1");
        var content = await EasyTestFile.LoadAsText(_testFileSettings.SetExtension(type));

        // act
        RepositoryActionConfiguration result = SutDeserialize(content, type);

        // assert
        await Verifier.Verify(result, _verifySettings).IgnoreParametersForVerified(type);
    }

    [Fact]
    public async Task Deserialize_Sample2()
    {
        // arrange
        SerializationType type = SerializationType.Yaml;
        _testFileSettings.UseFileName("Sample2");
        var content = await EasyTestFile.LoadAsText(_testFileSettings.SetExtension(type));

        // act
        RepositoryActionConfiguration result = SutDeserialize(content, type);

        // assert
        await Verifier.Verify(result, _verifySettings).IgnoreParametersForVerified(type);
    }

    [Fact]
    public async Task Deserialize_Sample3()
    {
        // arrange
        SerializationType type = SerializationType.Yaml;
        _testFileSettings.UseFileName("Sample3");
        var content = await EasyTestFile.LoadAsText(_testFileSettings.SetExtension(type));

        // act
        RepositoryActionConfiguration result = SutDeserialize(content, type);

        // assert
        await Verifier.Verify(result, _verifySettings).IgnoreParametersForVerified(type);
    }

    [Fact]
    public async Task Deserialize_VariableObject1()
    {
        // arrange
        SerializationType type = SerializationType.Yaml;
        _testFileSettings.UseFileName("VariableObject1");
        var content = await EasyTestFile.LoadAsText(_testFileSettings.SetExtension(type));

        // act
        RepositoryActionConfiguration result = SutDeserialize(content, type);

        // assert
        await Verifier.Verify(result, _verifySettings).IgnoreParametersForVerified(type);
    }

    [Fact]
    public async Task Deserialize_VariableObject2()
    {
        // arrange
        SerializationType type = SerializationType.Yaml;
        _testFileSettings.UseFileName("VariableObject2");
        var content = await EasyTestFile.LoadAsText(_testFileSettings.SetExtension(type));

        // act
        RepositoryActionConfiguration result = SutDeserialize(content, type);

        // assert
        await Verifier.Verify(result, _verifySettings).IgnoreParametersForVerified(type);
    }

    [Fact]
    public async Task Deserialize_ForEach1()
    {
        // arrange
        SerializationType type = SerializationType.Yaml;
        _testFileSettings.UseFileName("ForEach1");
        var content = await EasyTestFile.LoadAsText(_testFileSettings.SetExtension(type));

        // act
        RepositoryActionConfiguration result = SutDeserialize(content, type);

        // assert
        await Verifier.Verify(result, _verifySettings).IgnoreParametersForVerified(type);
    }

    [Fact]
    public async Task Deserialize_ForEach2()
    {
        // arrange
        SerializationType type = SerializationType.Yaml;
        _testFileSettings.UseFileName("ForEach2");
        var content = await EasyTestFile.LoadAsText(_testFileSettings.SetExtension(type));

        // act
        RepositoryActionConfiguration result = SutDeserialize(content, type);

        // assert
        await Verifier.Verify(result, _verifySettings).IgnoreParametersForVerified(type);
    }

    [Fact]
    public async Task Deserialize_ForEach3()
    {
        // arrange
        SerializationType type = SerializationType.Yaml;
        _testFileSettings.UseFileName("ForEach3");
        var content = await EasyTestFile.LoadAsText(_testFileSettings.SetExtension(type));

        // act
        RepositoryActionConfiguration result = SutDeserialize(content, type);

        // assert
        await Verifier.Verify(result, _verifySettings).IgnoreParametersForVerified(type);
    }

    [Fact]
    public void EmptyFile_ShouldThrow()
    {
        // arrange

        // act
        Func<RepositoryActionConfiguration> act = () => _ = _sutJson.Deserialize(string.Empty);

        // assert
        _ = act.Should().Throw<JsonReaderException>().WithMessage("Error reading JObject from JsonReader. Path '', line 0, position 0.");
    }
}