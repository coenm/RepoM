namespace RepoM.Api.Tests.IO.ModuleBasedRepositoryActionProvider;

using System.Threading.Tasks;
using EasyTestFile;
using EasyTestFileXunit;
using RepoM.Api.IO.ModuleBasedRepositoryActionProvider.Data;
using RepoM.Api.IO.ModuleBasedRepositoryActionProvider.Deserialization;
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
        _sut = new YamlDynamicRepositoryActionDeserializer(DynamicRepositoryActionDeserializerFactory.Create());

        _testFileSettings = new EasyTestFileSettings();
        _testFileSettings.UseDirectory("DocumentationFiles");
        _testFileSettings.UseExtension("yaml");

        _verifySettings = new VerifySettings();
        _verifySettings.UseDirectory("DocumentationFilesVerified");
    }

    [Theory]
    [InlineData("RepositoryActions01Base")]
    [InlineData("JustText01")]
    [InlineData("GitCheckout01")]
    [InlineData("GitFetch01")]
    [InlineData("GitPull01")]
    [InlineData("GitPush01")]
    [InlineData("browse-repository01")]
    [InlineData("Separator@1")]
    [InlineData("Browser@1")]
    [InlineData("Folder@1")]
    [InlineData("AssociateFile01")]
    [InlineData("Command01")]
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