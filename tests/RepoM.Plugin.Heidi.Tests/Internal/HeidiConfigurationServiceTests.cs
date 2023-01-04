namespace RepoM.Plugin.Heidi.Tests.Internal;

using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Threading;
using System.Threading.Tasks;
using FakeItEasy;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using RepoM.Plugin.Heidi.Internal;
using RepoM.Plugin.Heidi.Internal.Config;
using Xunit;

public class HeidiConfigurationServiceTests
{
    private readonly ILogger _logger;
    private readonly IFileSystem _fileSystem;
    private readonly IHeidiPortableConfigReader _configReader;
    private readonly IHeidiSettings _heidiSettings;
    private readonly IFileSystemWatcher _fileWatcher;
    private readonly HeidiConfigurationService _sut;

    public HeidiConfigurationServiceTests()
    {
        _logger = NullLogger.Instance;
        _fileSystem = A.Fake<IFileSystem>();
        _configReader = A.Fake<IHeidiPortableConfigReader>();
        _heidiSettings = A.Fake<IHeidiSettings>();
        _fileWatcher = A.Fake<IFileSystemWatcher>();
        _sut = new HeidiConfigurationService(_logger, _fileSystem, _configReader, _heidiSettings);

        A.CallTo(() => _fileSystem.FileSystemWatcher.New(A<string>._, A<string>._)).Returns(_fileWatcher);
        A.CallTo(() => _heidiSettings.ConfigFilename).Returns("heidi.portable.txt");
        A.CallTo(() => _heidiSettings.ConfigPath).Returns("C:\\heidi\\");
        A.CallTo(() => _fileSystem.File.Exists("C:\\heidi\\heidi.portable.txt")).Returns(true);
    }

    [Fact]
    public async Task InitializeAsync_ShouldReadHeidiFile_WhenFileExists()
    {
        var mre = new ManualResetEvent(false);
        A.CallTo(() => _fileSystem.File.Exists("C:\\heidi\\heidi.portable.txt")).Returns(true);
        A.CallTo(() => _configReader.ReadConfigsAsync("C:\\heidi\\heidi.portable.txt"))
         .Invokes(() => mre.Set())
         .Returns(Task.FromResult(new Dictionary<string, RepomHeidiConfig>()));

        // act
        await _sut.InitializeAsync();
        mre.WaitOne(TimeSpan.FromSeconds(2));

        // assert
        A.CallTo(() => _fileSystem.FileSystemWatcher.New(A<string>._, A<string>._)).MustHaveHappened();
        A.CallTo(() => _configReader.ReadConfigsAsync("C:\\heidi\\heidi.portable.txt")).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task InitializeAsync_ShouldNotReadHeidiFile_WhenFileNotExists()
    {
        var mre = new ManualResetEvent(false);
        A.CallTo(() => _fileSystem.File.Exists(A<string>._)).Returns(false);
        A.CallTo(() => _configReader.ReadConfigsAsync(A<string>._))
         .Invokes(() => mre.Set())
         .Returns(Task.FromResult(new Dictionary<string, RepomHeidiConfig>()));

        // act
        await _sut.InitializeAsync();
        mre.WaitOne(TimeSpan.FromSeconds(2));

        // assert
        A.CallTo(() => _configReader.ReadConfigsAsync(A<string>._)).MustNotHaveHappened();
    }

    [Fact]
    public async Task InitializeAsync_ShouldNotSubscribe_WhenFileNotExists()
    {
        A.CallTo(() => _fileSystem.File.Exists(A<string>._)).Returns(false);

        // act
        await _sut.InitializeAsync();

        // assert
        A.CallTo(() => _fileSystem.FileSystemWatcher.New(A<string>._, A<string>._)).MustNotHaveHappened();
    }
}