namespace RepoM.Api.Tests.Plugins;

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using FakeItEasy;
using FluentAssertions;
using RepoM.Api.Plugins;
using Xunit;

public class PluginFinderTests
{
    private readonly MockFileSystem _fileSystem;
    private readonly IHmacService _hmacService;

    public PluginFinderTests()
    {
        _fileSystem = new MockFileSystem(
            new Dictionary<string, MockFileData>
            {
                { "d:\\dummy\\bin\\net6\\release\\abc.dll", new MockFileData("bla") },
            });
            
        _hmacService = A.Fake<IHmacService>();
    }

    [Fact]
    public void Ctor_ShouldThrow_WhenArgumentIsNull()
    {
        // arrange

        // act
        Action act1 = () => _ = new PluginFinder(A.Dummy<IFileSystem>(), null!);
        Action act2 = () => _ = new PluginFinder(null!, A.Dummy<IHmacService>());

        // assert
        act1.Should().ThrowExactly<ArgumentNullException>();
        act2.Should().ThrowExactly<ArgumentNullException>();
    }

    [Fact]
    public void FindPlugins_ShouldReturnEmpty_WhenNoPluginDllsAvailable()
    {
        // arrange
        var sut = new PluginFinder(_fileSystem, _hmacService);

        // act
        IEnumerable<PluginInfo> result = sut.FindPlugins("d:\\dummy\\");

        // assert
        _ = result.Should().BeEmpty();
    }

    // [Fact]
    // public void FindPlugins_ShouldReturnPluginData_WhenPluginAvailable()
    // {
    //     // arrange
    //     A.CallTo(() => _hmacService.GetHmac(A<Stream>._)).Returns(new byte[] { 0x01, 0x02, });
    //     _fileSystem.AddFile("d:\\dummy\\RepoM.Plugin.Abc.dll", new MockFileData(Array.Empty<byte>()));
    //     var sut = new PluginFinder(_fileSystem, _hmacService);
    //
    //     // act
    //     IEnumerable<PluginInfo> result = sut.FindPlugins("d:\\dummy\\");
    //
    //     // assert
    //     _ = result.Should().BeEquivalentTo(new PluginInfo[] {new PluginInfo("d:\\dummy\\RepoM.Plugin.Abc.dll", null, new byte[] { 0x01, 0x02, }), });
    // }
}