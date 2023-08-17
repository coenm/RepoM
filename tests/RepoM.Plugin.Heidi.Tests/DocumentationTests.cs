namespace RepoM.Plugin.Heidi.Tests;

using System.Threading.Tasks;
using EasyTestFile;
using EasyTestFileXunit;
using RepoM.Api.IO.ModuleBasedRepositoryActionProvider.Data;
using RepoM.Api.IO.ModuleBasedRepositoryActionProvider.Deserialization;
using RepoM.Plugin.Heidi.Tests.TestFramework;
using VerifyTests;
using VerifyXunit;
using Xunit;

[UsesEasyTestFile]
[UsesVerify]
public class DocumentationTests
{
    private readonly EasyTestFileSettings _testFileSettings;
    private readonly VerifySettings _verifySettings;
    private readonly YamlDynamicRepositoryActionDeserializer _sut;

    public DocumentationTests()
    {
        _sut = DynamicRepositoryActionDeserializerFactory.Create();

        _testFileSettings = new EasyTestFileSettings();
        _testFileSettings.UseDirectory("DocumentationFiles");
        _testFileSettings.UseExtension("yaml");

        _verifySettings = new VerifySettings();
        _verifySettings.UseDirectory("DocumentationFilesVerified");
    }

    [Theory]
    [InlineData("HeidiDatabases")]
    public async Task Deserialize_Documentation(string filename)
    {
        // arrange
        _testFileSettings.UseFileName(filename);
        var content = await EasyTestFile.LoadAsText(_testFileSettings);

        // act
        RepositoryActionConfiguration result = _sut.Deserialize(content);

        // assert
        await Verifier.Verify(result, _verifySettings).UseTextForParameters(filename);
    }
}