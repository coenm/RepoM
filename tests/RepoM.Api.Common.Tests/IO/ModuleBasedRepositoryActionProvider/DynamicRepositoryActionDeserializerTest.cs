namespace RepoM.Api.Common.Tests.IO.ModuleBasedRepositoryActionProvider;

using System;
using System.Dynamic;
using System.Threading.Tasks;
using EasyTestFile;
using EasyTestFileXunit;
using FluentAssertions;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using RepoM.Api.Common.IO.ModuleBasedRepositoryActionProvider;
using RepoM.Api.Common.IO.ModuleBasedRepositoryActionProvider.Data;
using VerifyTests;
using VerifyXunit;
using Xunit;
using Xunit.Abstractions;
using XunitEnumMemberData;

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
        if (type == SerializationType.Json)
        {
            var expConverter = new ExpandoObjectConverter();
            dynamic deserializedObject = JsonConvert.DeserializeObject<ExpandoObject>(rawContent, expConverter)!;
            
            var serializer = new YamlDotNet.Serialization.Serializer();
            string yaml = serializer.Serialize(deserializedObject);

            _outputHelper.WriteLine(string.Empty);
            _outputHelper.WriteLine("----------------------------------------");
            _outputHelper.WriteLine(rawContent);
            _outputHelper.WriteLine("----------------------------------------");
            _outputHelper.WriteLine(string.Empty);
            _outputHelper.WriteLine("----------------------------------------");
            _outputHelper.WriteLine(yaml);
            _outputHelper.WriteLine("----------------------------------------");
            _outputHelper.WriteLine(string.Empty);
        }

        return type switch
            {
                SerializationType.Json => _sutJson.Deserialize(rawContent),
                SerializationType.Yaml => _sutYaml.Deserialize(rawContent),
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
            };
    }

    [Theory]
    [EnumMemberData(typeof(SerializationType))]
    public async Task Deserialize_ShouldReturnEmptyObject_WhenContentIsEmpty(SerializationType type)
    {
        // arrange
        _testFileSettings.UseFileName("Empty");
        var content = await EasyTestFile.LoadAsText(_testFileSettings.SetExtension(type));

        // act
        RepositoryActionConfiguration result = SutDeserialize(content, type);

        // assert
        await Verifier.Verify(result, _verifySettings).IgnoreParametersForVerified(type);
    }

    [Theory]
    [EnumMemberData(typeof(SerializationType))]
    public async Task Deserialize_ShouldReturnEmptyObject_WhenVersionIsUnknown(SerializationType type)
    {
        // arrange
        _testFileSettings.UseFileName("Version100");
        var content = await EasyTestFile.LoadAsText(_testFileSettings.SetExtension(type));

        // act
        RepositoryActionConfiguration result = SutDeserialize(content, type);

        // assert
        await Verifier.Verify(result, _verifySettings).IgnoreParametersForVerified(type);
    }

    [Theory]
    [EnumMemberData(typeof(SerializationType))]
    public async Task Deserialize_ShouldReturnObjectWithVariables_WhenContentIsVariablesOnly(SerializationType type)
    {
        // arrange
        _testFileSettings.UseFileName("VariablesOnly1");
        var content = await EasyTestFile.LoadAsText(_testFileSettings.SetExtension(type));

        // act
        RepositoryActionConfiguration result = SutDeserialize(content, type);

        // assert
        await Verifier.Verify(result, _verifySettings).IgnoreParametersForVerified(type);
    }

    [Theory]
    [EnumMemberData(typeof(SerializationType))]
    public async Task Deserialize_ShouldReturnObjectWithRepositoryTags_WhenContentIsRepositoryTags1(SerializationType type)
    {
        // arrange
        _testFileSettings.UseFileName("RepositoryTags1");
        var content = await EasyTestFile.LoadAsText(_testFileSettings.SetExtension(type));

        // act
        RepositoryActionConfiguration result = SutDeserialize(content, type);

        // assert
        await Verifier.Verify(result, _verifySettings).IgnoreParametersForVerified(type);
    }

    [Theory]
    [EnumMemberData(typeof(SerializationType))]
    public async Task Deserialize_ShouldReturnObjectWithLatestTags_WhenContentHasDoubleTags(SerializationType type)
    {
        // arrange
        _testFileSettings.UseFileName("RepositoryTagsDouble");
        var content = await EasyTestFile.LoadAsText(_testFileSettings.SetExtension(type));

        // act
        RepositoryActionConfiguration result = SutDeserialize(content, type);

        // assert
        await Verifier.Verify(result, _verifySettings).IgnoreParametersForVerified(type);
    }

    [Theory]
    [EnumMemberData(typeof(SerializationType))]
    public async Task Deserialize_ShouldReturnObjectWithRepositoryTags_WhenContentIsRepositoryTags2(SerializationType type)
    {
        // arrange
        _testFileSettings.UseFileName("RepositoryTags2");
        var content = await EasyTestFile.LoadAsText(_testFileSettings.SetExtension(type));

        // act
        RepositoryActionConfiguration result = SutDeserialize(content, type);

        // assert
        await Verifier.Verify(result, _verifySettings)
                      .UseMethodName(nameof(Deserialize_ShouldReturnObjectWithRepositoryTags_WhenContentIsRepositoryTags1))
                      .IgnoreParametersForVerified(type);
    }

    [Theory]
    [EnumMemberData(typeof(SerializationType))]
    public async Task Deserialize_ShouldReturnObjectWithRepositoryTags_WhenContentIsRepositoryTags3(SerializationType type)
    {
        // arrange
        _testFileSettings.UseFileName("RepositoryTags3");
        var content = await EasyTestFile.LoadAsText(_testFileSettings.SetExtension(type));

        // act
        RepositoryActionConfiguration result = SutDeserialize(content, type);

        // assert
        await Verifier.Verify(result, _verifySettings).IgnoreParametersForVerified(type);
    }

    [Theory]
    [EnumMemberData(typeof(SerializationType))]
    public async Task Deserialize_ShouldReturnObjectWithRepositoryActions_WhenContentIsRepositoryActions1(SerializationType type)
    {
        // arrange
        _testFileSettings.UseFileName("RepositoryActions1");
        var content = await EasyTestFile.LoadAsText(_testFileSettings.SetExtension(type));

        // act
        RepositoryActionConfiguration result = SutDeserialize(content, type);

        // assert
        await Verifier.Verify(result, _verifySettings).IgnoreParametersForVerified(type);
    }

    [Theory]
    [EnumMemberData(typeof(SerializationType))]
    public async Task Deserialize_ShouldReturnObjectWithRepositoryActions_WhenContentIsRepositoryActions2(SerializationType type)
    {
        // arrange
        _testFileSettings.UseFileName("RepositoryActions2");
        var content = await EasyTestFile.LoadAsText(_testFileSettings.SetExtension(type));

        // act
        RepositoryActionConfiguration result = SutDeserialize(content, type);

        // assert
        await Verifier.Verify(result, _verifySettings)
                      .IgnoreParametersForVerified(type)
                      .UseMethodName(nameof(Deserialize_ShouldReturnObjectWithRepositoryActions_WhenContentIsRepositoryActions1));
    }

    [Theory]
    [EnumMemberData(typeof(SerializationType))]
    public async Task Deserialize_ShouldReturnObjectWithRepositoryActions_WhenContentIsRepositoryActions3(SerializationType type)
    {
        // arrange
        _testFileSettings.UseFileName("RepositoryActions3");
        var content = await EasyTestFile.LoadAsText(_testFileSettings.SetExtension(type));

        // act
        RepositoryActionConfiguration result = SutDeserialize(content, type);

        // assert
        await Verifier.Verify(result, _verifySettings).IgnoreParametersForVerified(type);
    }

    [Theory]
    [EnumMemberData(typeof(SerializationType))]
    public async Task Deserialize_ShouldReturnObjectWithRedirect_WhenContentIsRedirect1(SerializationType type)
    {
        // arrange
        _testFileSettings.UseFileName("Redirect1");
        var content = await EasyTestFile.LoadAsText(_testFileSettings.SetExtension(type));

        // act
        RepositoryActionConfiguration result = SutDeserialize(content, type);

        // assert
        await Verifier.Verify(result, _verifySettings).IgnoreParametersForVerified(type);
    }

    [Theory]
    [EnumMemberData(typeof(SerializationType))]
    public async Task Deserialize_ShouldReturnObjectWithRedirect_WhenContentIsRedirect2(SerializationType type)
    {
        // arrange
        _testFileSettings.UseFileName("Redirect2");
        var content = await EasyTestFile.LoadAsText(_testFileSettings.SetExtension(type));

        // act
        RepositoryActionConfiguration result = SutDeserialize(content, type);

        // assert
        await Verifier.Verify(result, _verifySettings)
                      .IgnoreParametersForVerified(type)
                      .UseMethodName(nameof(Deserialize_ShouldReturnObjectWithRedirect_WhenContentIsRedirect1));
    }

    [Theory]
    [EnumMemberData(typeof(SerializationType))]
    public async Task Deserialize_ShouldReturnObjectWithRedirect_WhenContentIsRedirect3(SerializationType type)
    {
        // arrange
        _testFileSettings.UseFileName("Redirect3");
        var content = await EasyTestFile.LoadAsText(_testFileSettings.SetExtension(type));

        // act
        RepositoryActionConfiguration result = SutDeserialize(content, type);

        // assert
        await Verifier.Verify(result, _verifySettings).IgnoreParametersForVerified(type);
    }

    [Theory]
    [EnumMemberData(typeof(SerializationType))]
    public async Task Deserialize_ShouldReturnObject_WhenContentIsRepositorySpecificEnvFile1(SerializationType type)
    {
        // arrange
        _testFileSettings.UseFileName("RepositorySpecificEnvFile1");
        var content = await EasyTestFile.LoadAsText(_testFileSettings.SetExtension(type));

        // act
        RepositoryActionConfiguration result = SutDeserialize(content, type);

        // assert
        await Verifier.Verify(result, _verifySettings).IgnoreParametersForVerified(type);
    }

    [Theory]
    [EnumMemberData(typeof(SerializationType))]
    public async Task Deserialize_ShouldReturnObject_WhenContentIsRepositorySpecificConfigFile1(SerializationType type)
    {
        // arrange
        _testFileSettings.UseFileName("RepositorySpecificConfigFile1");
        var content = await EasyTestFile.LoadAsText(_testFileSettings.SetExtension(type));

        // act
        RepositoryActionConfiguration result = SutDeserialize(content, type);

        // assert
        await Verifier.Verify(result, _verifySettings).IgnoreParametersForVerified(type);
    }

    [Theory]
    [EnumMemberData(typeof(SerializationType))]
    public async Task Deserialize_Sample1(SerializationType type)
    {
        // arrange
        _testFileSettings.UseFileName("Sample1");
        var content = await EasyTestFile.LoadAsText(_testFileSettings.SetExtension(type));

        // act
        RepositoryActionConfiguration result = SutDeserialize(content, type);

        // assert
        await Verifier.Verify(result, _verifySettings).IgnoreParametersForVerified(type);
    }

    [Theory]
    [EnumMemberData(typeof(SerializationType))]
    public async Task Deserialize_Sample2(SerializationType type)
    {
        // arrange
        _testFileSettings.UseFileName("Sample2");
        var content = await EasyTestFile.LoadAsText(_testFileSettings.SetExtension(type));

        // act
        RepositoryActionConfiguration result = SutDeserialize(content, type);

        // assert
        await Verifier.Verify(result, _verifySettings).IgnoreParametersForVerified(type);
    }

    [Theory]
    [EnumMemberData(typeof(SerializationType))]
    public async Task Deserialize_Sample3(SerializationType type)
    {
        // arrange
        _testFileSettings.UseFileName("Sample3");
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