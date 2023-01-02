namespace RepoM.Plugin.Heidi.Tests.Internal;

using System.IO.Abstractions.TestingHelpers;
using System.Threading.Tasks;
using EasyTestFile;
using EasyTestFileXunit;
using Microsoft.Extensions.Logging.Abstractions;
using RepoM.Api.Tests.IO.ModuleBasedRepositoryActionProvider;
using RepoM.Plugin.Heidi.Internal;
using VerifyTests;
using VerifyXunit;
using Xunit;

[UsesEasyTestFile]
[UsesVerify]
public class HeidiPortableConfigReaderTests
{
    private readonly HeidiPortableConfigReader _sut;
    private readonly EasyTestFileSettings _testFileSettings;
    private readonly VerifySettings _verifySettings;
    private readonly MockFileSystem _mockFileSystem;

    public HeidiPortableConfigReaderTests()
    {
        _testFileSettings = new EasyTestFileSettings();
        _testFileSettings.UseDirectory("TestFiles");
        _testFileSettings.UseExtension("txt");
        
        _verifySettings = new VerifySettings();
        _verifySettings.UseDirectory("Verified");
        _mockFileSystem = new MockFileSystem();
        _sut = new HeidiPortableConfigReader(_mockFileSystem, NullLogger.Instance);
    }

    [Fact]
    public async Task Abc()
    {
        var xx = @$"Servers\BDO\MSS - BDODT-D\Comment<|||>1<|||>MigrationStatusService<{{{{{{><}}}}}}> <{{{{{{><}}}}}}>#REPOM_START#{{""Repositories"":[""RepoM""],""Order"":12,""Name"":""CoProf"",""Environment"":""D"",""Application"":""ap""}}#REPOM_END#";
        

        // arrange
        _testFileSettings.UseFileName("heidi1");
        var content = await EasyTestFile.LoadAsText(_testFileSettings);
        _mockFileSystem.AddFile("file2.txt", content);
        // _mockFileSystem.AddFile("file1.txt", @$"Servers\BDO\MSS - BDODT-D\Comment<|||>1<|||>MigrationStatusService<{{{{{{><}}}}}}> <{{{{{{><}}}}}}>#REPOM_START#{{""Repositories"":[""RepoM""],""Order"":12,""Name"":""CoProf"",""Environment"":""D"",""Application"":""ap"",""Version"":0,""HeidiKey"":null}}#REPOM_END#");
        _mockFileSystem.AddFile("file1.txt", @$"Servers\BDO\MSS - BDODT-D\Comment<|||>1<|||>MigrationStatusService<{{{{{{><}}}}}}> <{{{{{{><}}}}}}>#REPOM_START#{{""Repositories"":[""RepoM""],""Order"":12,""Name"":""CoProf"",""Environment"":""D"",""Application"":""ap""}}#REPOM_END#");
        // Servers\BDO\MSS - BDODT-D\Comment<|||>1<|||>MigrationStatusService<{{{><}}}> <{{{><}}}>#REPOM_START#{"Repositories":["RepoM"],"Order":12,"Name":"CoProf","Environment":"D","Application":"ap","Version":0,"HeidiKey":null}#REPOM_END#
        // {"Repositories":["RepoM"],"Order":12,"Name":"CoProf","Environment":"D","Application":"ap","Version":0,"HeidiKey":null}

        // act
        var result = await _sut.ReadConfigsAsync("file1.txt");

        // assert
        _ = await Verifier.Verify(result);
    }
}