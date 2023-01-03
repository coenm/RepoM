namespace RepoM.Plugin.Heidi.Tests.Internal;

using System.Collections.Generic;
using System.IO.Abstractions.TestingHelpers;
using System.Threading.Tasks;
using EasyTestFile;
using EasyTestFileXunit;
using Microsoft.Extensions.Logging.Abstractions;
using RepoM.Plugin.Heidi.Internal;
using RepoM.Plugin.Heidi.Internal.Config;
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

    [Fact(Skip = "Tmp")]
    public async Task RealFile()
    {
        // arrange
        _testFileSettings.UseFileName("heidi1");
        var content = await EasyTestFile.LoadAsText(_testFileSettings);
        _mockFileSystem.AddFile("file1.txt", content);

        // act
        Dictionary<string, RepomHeidiConfig> result = await _sut.ReadConfigsAsync("file1.txt");

        // assert
        _ = await Verifier.Verify(result, _verifySettings);
    }

    [Fact]
    public async Task SingleLine()
    {
        // arrange
        _mockFileSystem.AddFile("file1.txt", @$"Servers\RepoM\MSS - DT-D\Comment<|||>1<|||>RepoM<{{{{{{><}}}}}}> <{{{{{{><}}}}}}>#REPOM_START#{{""Repositories"":[""RepoM""],""Order"":12,""Name"":""cp"",""Environment"":""D"",""Application"":""ap""}}#REPOM_END#");
       
        // act
        Dictionary<string, RepomHeidiConfig> result = await _sut.ReadConfigsAsync("file1.txt");

        // assert
        _ = await Verifier.Verify(result, _verifySettings);
    }
}