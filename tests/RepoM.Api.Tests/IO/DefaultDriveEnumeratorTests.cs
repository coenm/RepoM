namespace RepoM.Api.Tests.IO;

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using FakeItEasy;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using RepoM.Api.Common;
using RepoM.Api.IO;
using Xunit;

public class DefaultDriveEnumeratorTests
{
    private readonly IAppSettingsService _appSettings;
    private readonly MockFileSystem _fileSystem;

    public DefaultDriveEnumeratorTests()
    {
        _appSettings = A.Fake<IAppSettingsService>();
        _fileSystem = new MockFileSystem();
    }

    [Fact]
    public void Ctor_ShouldThrow_WhenArgumentIsNull()
    {
        // arrange

        // act
        Action act1 = () => _ = new DefaultDriveEnumerator(A.Dummy<IAppSettingsService>(), A.Dummy<IFileSystem>(), null!);
        Action act2 = () => _ = new DefaultDriveEnumerator(A.Dummy<IAppSettingsService>(), null!, A.Dummy<ILogger>());
        Action act3 = () => _ = new DefaultDriveEnumerator(null!, A.Dummy<IFileSystem>(), A.Dummy<ILogger>());

        // assert
        act1.Should().ThrowExactly<ArgumentNullException>();
        act2.Should().ThrowExactly<ArgumentNullException>();
        act3.Should().ThrowExactly<ArgumentNullException>();
    }

    [Fact]
    public void GetPaths_ShouldReturnPreconfiguredDirectories_WhenExist1()
    {
        // arrange
        _appSettings.ReposRootDirectories = new List<string> { "d:\\abc\\", };
        _fileSystem.AddDirectory("d:\\abc");
        var sut = new DefaultDriveEnumerator(_appSettings, _fileSystem, NullLogger.Instance);

        // act
        var result = sut.GetPaths();

        // assert
        result.Should().BeEquivalentTo("d:\\abc\\");
    }

    [Fact]
    public void GetPaths_ShouldSkipPredefinedDirectories_WhenNotExists()
    {
        // arrange
        _appSettings.ReposRootDirectories = new List<string> { "d:\\abc\\", "d:\\abc-not-exist\\", };
        _fileSystem.AddDirectory("d:\\abc");
        var sut = new DefaultDriveEnumerator(_appSettings, _fileSystem, NullLogger.Instance);

        // act
        var result = sut.GetPaths();

        // assert
        result.Should().BeEquivalentTo("d:\\abc\\");
    }

    [Fact]
    public void GetPaths_ShouldReturnHardDrives_WhenNoExistingPredefinedPathsFound()
    {
        // arrange
        _appSettings.ReposRootDirectories = new List<string> { "d:\\abc-not-exist\\", };
        IFileSystem fileSystem = A.Fake<IFileSystem>();
        IDriveInfoFactory driveInfo = A.Fake<IDriveInfoFactory>();
        A.CallTo(() => driveInfo.GetDrives()).Returns(
            new []
                {
                    CreateDriveInfo("C:\\", DriveType.Fixed),
                    CreateDriveInfo("D:\\", DriveType.Fixed),
                    CreateDriveInfo("E:\\", DriveType.Removable),
                });
        A.CallTo(() => fileSystem.DriveInfo).Returns(driveInfo);
        
        var sut = new DefaultDriveEnumerator(_appSettings, fileSystem, NullLogger.Instance);

        // act
        var result = sut.GetPaths();

        // assert
        result.Should().BeEquivalentTo("C:\\", "D:\\");
    }

    private static IDriveInfo CreateDriveInfo(string name, DriveType type)
    {
        IDriveInfo result = A.Fake<IDriveInfo>(x => x.Strict());
        A.CallTo(() => result.DriveType).Returns(type);
        A.CallTo(() => result.Name).Returns(name);
        IDirectoryInfo di = A.Fake<IDirectoryInfo>();
        A.CallTo(() => di.FullName).Returns(name);
        A.CallTo(() => result.RootDirectory).Returns(di);
        return result;
    }
}