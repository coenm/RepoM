namespace RepoM.Api.Tests.IO.ModuleBasedRepositoryActionProvider.Action;

using System.Threading.Tasks;
using EasyTestFile;
using EasyTestFileXunit;
using FluentAssertions;
using RepoM.Api.IO.ModuleBasedRepositoryActionProvider.ActionDeserializers;
using RepoM.Api.IO.ModuleBasedRepositoryActionProvider.Data.Actions;
using RepoM.Api.IO.ModuleBasedRepositoryActionProvider.Deserialization;
using VerifyTests;
using VerifyXunit;
using Xunit;

[UsesEasyTestFile]
[UsesVerify]
public class SeparatorV1Test
{
    private readonly JsonDynamicRepositoryActionDeserializer _sut;
    private readonly EasyTestFileSettings _testFileSettings;
    private readonly VerifySettings _verifySettings;

    public SeparatorV1Test()
    {
        _sut = DynamicRepositoryActionDeserializerFactory.CreateWithDeserializer(new ActionSeparatorV1Deserializer());

        _testFileSettings = new EasyTestFileSettings();
        _testFileSettings.UseDirectory("TestFiles");
        _testFileSettings.UseExtension("json");

        _verifySettings = new VerifySettings();
        _verifySettings.UseDirectory("Verified");
    }

    [Fact]
    public async Task Deserialize_SeparatorV1()
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
        _testFileSettings.UseMethodName(nameof(Deserialize_SeparatorV1));
        var content = await EasyTestFile.LoadAsText(_testFileSettings);

        // act
        var result = _sut.Deserialize(content);

        // assert
        _ = result.ActionsCollection.Actions.Should().AllBeOfType<RepositoryActionSeparatorV1>();
    }
}