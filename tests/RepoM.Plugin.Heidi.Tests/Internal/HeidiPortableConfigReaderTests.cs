namespace RepoM.Plugin.Heidi.Tests.Internal;

using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using System.Threading.Tasks;
using EasyTestFile;
using EasyTestFileXunit;
using FakeItEasy;
using FluentAssertions;
using Microsoft.Extensions.Logging;
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
        IHeidiPasswordDecoder passwordDecoder = new DummyDecoder();
        _sut = new HeidiPortableConfigReader(_mockFileSystem, NullLogger.Instance, passwordDecoder);
    }

    [Fact]
    public void Ctor_ShouldThrow_WhenArgumentIsNull()
    {
        // arrange
        var fileSystem = A.Dummy<IFileSystem>();
        var logger = A.Dummy<ILogger>();
        var heidiPasswordDecoder = A.Dummy<IHeidiPasswordDecoder>();

        // act
        Action act1 = () => _ = new HeidiPortableConfigReader(fileSystem, logger, null!);
        Action act2 = () => _ = new HeidiPortableConfigReader(fileSystem, null!, heidiPasswordDecoder);
        Action act3 = () => _ = new HeidiPortableConfigReader(null!, logger, heidiPasswordDecoder);

        // assert
        act1.Should().Throw<ArgumentNullException>();
        act2.Should().Throw<ArgumentNullException>();
        act3.Should().Throw<ArgumentNullException>();
    }
    
    [Fact(Skip = "Tmp")]
    public async Task RealFile()
    {
        // arrange
        _testFileSettings.UseFileName("heidi1");
        var content = await EasyTestFile.LoadAsText(_testFileSettings);
        _mockFileSystem.AddFile("file1.txt", content);

        // act
        List<HeidiSingleDatabaseConfiguration> result = await _sut.ParseAsync("file1.txt");

        // assert
        _ = await Verifier.Verify(result, _verifySettings);
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData("rubbsh")]
    [InlineData("rubbsh rubbsh rubbsh rubbsh")]
    [InlineData("Servers\\rubbshfsdf sdfkjsfdlj ")]
    [InlineData("Servers\\")]
    [InlineData(" Servers\\Name\\Comment<|||>aa")]
    [InlineData("ServersName\\Comment<|||>value")]
    [InlineData("Server\\Name\\Comment<|||>value")]
    public async Task ReadConfigAsync_ShouldIgnoreInvalidInput(string input)
    {
        // arrange
        _mockFileSystem.AddFile("file.txt", input);

        // act
        var result = await _sut.ParseSingleLinesAsync("file.txt");

        // assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task S()
    {
        // arrange
        _testFileSettings.UseFileName("heidi2");
        var content = await EasyTestFile.LoadAsText(_testFileSettings);
        _mockFileSystem.AddFile("file1.txt", content);

        // act
        var result = await _sut.ParseSingleLinesAsync("file1.txt");

        // assert
        await Verifier.Verify(result, _verifySettings);
    }

    [Fact]
    public async Task S2()
    {
        // arrange
        _testFileSettings.UseFileName("heidi2");
        var content = await EasyTestFile.LoadAsText(_testFileSettings);
        _mockFileSystem.AddFile("file1.txt", content);
        
        // act
        var result = await _sut.ParseConfiguration2Async("file1.txt");

        // assert
        await Verifier.Verify(result, _verifySettings);
    }

    private class DummyDecoder : IHeidiPasswordDecoder
    {
        public string DecodePassword(ReadOnlySpan<char> input)
        {
            return "Decoded:" + input.ToString();
        }
    }
}