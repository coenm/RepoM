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

[UsesEasyTestFile]
public class DynamicRepositoryActionDeserializerTest
{
    private readonly EasyTestFileSettings _testFileSettings;
    private readonly VerifySettings _verifySettings;
    private readonly YamlDynamicRepositoryActionDeserializer _sut;

    public DynamicRepositoryActionDeserializerTest()
    {
        _sut = DynamicRepositoryActionDeserializerFactory.Create();

        _testFileSettings = new EasyTestFileSettings();
        _testFileSettings.UseDirectory("TestFiles");
        _testFileSettings.UseExtension("yaml");

        _verifySettings = new VerifySettings();
        _verifySettings.UseDirectory("Verified");
    }

    [Fact]
    public async Task Deserialize_ShouldReturnData_WhenContentIsEmptyVersion1()
    {
        // arrange
        _testFileSettings.UseFileName("Version1");
        var content = await EasyTestFile.LoadAsText(_testFileSettings);

        // act
        RepositoryActionConfiguration result = _sut.Deserialize(content);

        // assert
        await Verifier.Verify(result, _verifySettings);
    }
    
    [Fact]
    public async Task Deserialize_RepositorySpecificEnvFiles_1()
    {
        // arrange
        _testFileSettings.UseFileName("RepositorySpecificEnvFilesExplanation1");
        var content = await EasyTestFile.LoadAsText(_testFileSettings);

        // act
        RepositoryActionConfiguration result = _sut.Deserialize(content);

        // assert
        await Verifier.Verify(result, _verifySettings);
    }

    [Fact]
    public async Task Deserialize_RepositoryTagsFiles_1()
    {
        // arrange
        _testFileSettings.UseFileName("RepositoryTagsExplanation1");
        var content = await EasyTestFile.LoadAsText(_testFileSettings);

        // act
        RepositoryActionConfiguration result = _sut.Deserialize(content);

        // assert
        await Verifier.Verify(result, _verifySettings);
    }

    [Fact]
    public async Task Deserialize_ShouldReturnEmptyObject_WhenContentIsEmpty()
    {
        // arrange
        _testFileSettings.UseFileName("Empty");
        var content = await EasyTestFile.LoadAsText(_testFileSettings);

        // act
        RepositoryActionConfiguration result = _sut.Deserialize(content);

        // assert
        await Verifier.Verify(result, _verifySettings);
    }

    [Fact]
    public async Task Deserialize_ShouldReturnEmptyObject_WhenVersionIsUnknown()
    {
        // arrange
        _testFileSettings.UseFileName("Version100");
        var content = await EasyTestFile.LoadAsText(_testFileSettings);

        // act
        RepositoryActionConfiguration result = _sut.Deserialize(content);

        // assert
        await Verifier.Verify(result, _verifySettings);
    }

    [Fact]
    public async Task Deserialize_ShouldReturnObjectWithVariables_WhenContentIsVariablesOnly()
    {
        // arrange
        _testFileSettings.UseFileName("VariablesOnly1");
        var content = await EasyTestFile.LoadAsText(_testFileSettings);

        // act
        RepositoryActionConfiguration result = _sut.Deserialize(content);

        // assert
        await Verifier.Verify(result, _verifySettings);
    }

    [Fact]
    public async Task Deserialize_ShouldReturnObjectWithRepositoryTags_WhenContentIsRepositoryTags1()
    {
        // arrange
        _testFileSettings.UseFileName("RepositoryTags1");
        var content = await EasyTestFile.LoadAsText(_testFileSettings);

        // act
        RepositoryActionConfiguration result = _sut.Deserialize(content);

        // assert
        await Verifier.Verify(result, _verifySettings);
    }

    [Fact]
    public async Task Deserialize_ShouldReturnObjectWithLatestTags_WhenContentHasDoubleTags()
    {
        // arrange
        _testFileSettings.UseFileName("RepositoryTagsDouble");
        var content = await EasyTestFile.LoadAsText(_testFileSettings);

        // act
        RepositoryActionConfiguration result = _sut.Deserialize(content);

        // assert
        await Verifier.Verify(result, _verifySettings);
    }

    [Fact]
    public async Task Deserialize_ShouldReturnObjectWithRepositoryTags_WhenContentIsRepositoryTags2()
    {
        // arrange
        _testFileSettings.UseFileName("RepositoryTags2");
        var content = await EasyTestFile.LoadAsText(_testFileSettings);

        // act
        RepositoryActionConfiguration result = _sut.Deserialize(content);

        // assert
        await Verifier.Verify(result, _verifySettings)
                      .UseMethodName(nameof(Deserialize_ShouldReturnObjectWithRepositoryTags_WhenContentIsRepositoryTags1));
    }

    [Fact]
    public async Task Deserialize_ShouldReturnObjectWithRepositoryTags_WhenContentIsRepositoryTags3()
    {
        // arrange
        _testFileSettings.UseFileName("RepositoryTags3");
        var content = await EasyTestFile.LoadAsText(_testFileSettings);

        // act
        RepositoryActionConfiguration result = _sut.Deserialize(content);

        // assert
        await Verifier.Verify(result, _verifySettings);
    }

    [Fact]
    public async Task Deserialize_ShouldReturnObjectWithRedirect_WhenContentIsRedirect1()
    {
        // arrange
        _testFileSettings.UseFileName("Redirect1");
        var content = await EasyTestFile.LoadAsText(_testFileSettings);

        // act
        RepositoryActionConfiguration result = _sut.Deserialize(content);

        // assert
        await Verifier.Verify(result, _verifySettings);
    }

    [Fact]
    public async Task Deserialize_ShouldReturnObjectWithRedirect_WhenContentIsRedirect2()
    {
        // arrange
        _testFileSettings.UseFileName("Redirect2");
        var content = await EasyTestFile.LoadAsText(_testFileSettings);

        // act
        RepositoryActionConfiguration result = _sut.Deserialize(content);

        // assert
        await Verifier.Verify(result, _verifySettings)
                      .UseMethodName(nameof(Deserialize_ShouldReturnObjectWithRedirect_WhenContentIsRedirect1));
    }

    [Fact]
    public async Task Deserialize_ShouldReturnObjectWithRedirect_WhenContentIsRedirect3()
    {
        // arrange
        _testFileSettings.UseFileName("Redirect3");
        var content = await EasyTestFile.LoadAsText(_testFileSettings);

        // act
        RepositoryActionConfiguration result = _sut.Deserialize(content);

        // assert
        await Verifier.Verify(result, _verifySettings);
    }

    [Fact]
    public async Task Deserialize_ShouldReturnObject_WhenContentIsRepositorySpecificEnvFile1()
    {
        // arrange
        _testFileSettings.UseFileName("RepositorySpecificEnvFile1");
        var content = await EasyTestFile.LoadAsText(_testFileSettings);

        // act
        RepositoryActionConfiguration result = _sut.Deserialize(content);

        // assert
        await Verifier.Verify(result, _verifySettings);
    }

    [Fact]
    public async Task Deserialize_ShouldReturnObject_WhenContentIsRepositorySpecificConfigFile1()
    {
        // arrange
        _testFileSettings.UseFileName("RepositorySpecificConfigFile1");
        var content = await EasyTestFile.LoadAsText(_testFileSettings);

        // act
        RepositoryActionConfiguration result = _sut.Deserialize(content);

        // assert
        await Verifier.Verify(result, _verifySettings);
    }

    [Fact]
    public async Task Deserialize_Sample1()
    {
        // arrange
        _testFileSettings.UseFileName("Sample1");
        var content = await EasyTestFile.LoadAsText(_testFileSettings);

        // act
        RepositoryActionConfiguration result = _sut.Deserialize(content);

        // assert
        await Verifier.Verify(result, _verifySettings);
    }

    [Fact]
    public async Task Deserialize_VariableObject1()
    {
        // arrange
        _testFileSettings.UseFileName("VariableObject1");
        var content = await EasyTestFile.LoadAsText(_testFileSettings);

        // act
        RepositoryActionConfiguration result = _sut.Deserialize(content);

        // assert
        await Verifier.Verify(result, _verifySettings);
    }

    [Fact]
    public async Task Deserialize_VariableObject2()
    {
        // arrange
        _testFileSettings.UseFileName("VariableObject2");
        var content = await EasyTestFile.LoadAsText(_testFileSettings);

        // act
        RepositoryActionConfiguration result = _sut.Deserialize(content);

        // assert
        await Verifier.Verify(result, _verifySettings);
    }

    [Fact]
    public void EmptyFile_ShouldThrow()
    {
        // arrange

        // act
        Func<RepositoryActionConfiguration> act = () => _ = _sut.Deserialize(string.Empty);

        // assert
        _ = act.Should().Throw<JsonReaderException>().WithMessage("Error reading JObject from JsonReader. Path '', line 0, position 0.");
    }
}