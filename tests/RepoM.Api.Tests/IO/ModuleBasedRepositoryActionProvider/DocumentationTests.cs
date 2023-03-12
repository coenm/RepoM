namespace RepoM.Api.Tests.IO.ModuleBasedRepositoryActionProvider;

using System;
using System.Threading.Tasks;
using EasyTestFile;
using EasyTestFileXunit;
using RepoM.Api.IO.ModuleBasedRepositoryActionProvider.Data;
using RepoM.Api.IO.ModuleBasedRepositoryActionProvider.Deserialization;
using VerifyTests;
using VerifyXunit;
using Xunit;
using Xunit.Abstractions;
using XunitEnumMemberData;

[UsesEasyTestFile]
[UsesVerify]
public class DocumentationTests
{
    private readonly ITestOutputHelper _outputHelper;
    private readonly EasyTestFileSettings _testFileSettings;
    private readonly VerifySettings _verifySettings;
    private readonly YamlDynamicRepositoryActionDeserializer _sut;

    public DocumentationTests(ITestOutputHelper outputHelper)
    {
        _outputHelper = outputHelper ?? throw new ArgumentNullException(nameof(outputHelper));
        _sut = new YamlDynamicRepositoryActionDeserializer(DynamicRepositoryActionDeserializerFactory.Create());

        _testFileSettings = new EasyTestFileSettings();
        _testFileSettings.UseDirectory("DocumentationFiles");
        _testFileSettings.UseExtension("yaml");

        _verifySettings = new VerifySettings();
        _verifySettings.UseDirectory("DocumentationFilesVerified");
    }

    [Theory]
    [InlineData("RepositoryActions01Base")]
    public async Task Deserialize_Documentation(string filename)
    {
        // arrange
        _testFileSettings.UseFileName(filename);
        var content = await EasyTestFile.LoadAsText(_testFileSettings);

        // act
        RepositoryActionConfiguration result = _sut.Deserialize(content);

        // assert
        await Verifier.Verify(result, _verifySettings);
    }
}