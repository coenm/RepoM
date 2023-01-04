namespace RepoM.Plugin.Heidi.Tests.Internal;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.IO.Abstractions;
using System.Reflection.PortableExecutable;
using System.Threading;
using System.Threading.Tasks;
using ExpressionStringEvaluator.Methods.Linq;
using FakeItEasy;
using FakeItEasy.Core;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using RepoM.Plugin.Heidi.Internal;
using RepoM.Plugin.Heidi.Internal.Config;
using Xunit;

public class HeidiConfigurationServiceTests
{
    private const string FILENAME = "heidi.portable.txt";
    private const string PATH = "C:\\heidi\\";


    private readonly ILogger _logger;
    private readonly IFileSystem _fileSystem;
    private readonly IHeidiPortableConfigReader _configReader;
    private readonly IHeidiSettings _heidiSettings;
    private readonly IFileSystemWatcher _fileWatcher;
    private readonly HeidiConfigurationService _sut;
    private readonly ChangeEventDummyFileSystemWatcher _changeEventDummyFileSystemWatcher;

    public HeidiConfigurationServiceTests()
    {
        _changeEventDummyFileSystemWatcher = new ChangeEventDummyFileSystemWatcher();
        _fileWatcher = A.Fake<IFileSystemWatcher>(o => o.Wrapping(_changeEventDummyFileSystemWatcher));

        _logger = NullLogger.Instance;
        _fileSystem = A.Fake<IFileSystem>();
        _configReader = A.Fake<IHeidiPortableConfigReader>();
        _heidiSettings = A.Fake<IHeidiSettings>();
        _sut = new HeidiConfigurationService(_logger, _fileSystem, _configReader, _heidiSettings);

        A.CallTo(() => _fileSystem.FileSystemWatcher.New(A<string>._, A<string>._)).Returns(_fileWatcher);
        A.CallTo(() => _heidiSettings.ConfigFilename).Returns(FILENAME);
        A.CallTo(() => _heidiSettings.ConfigPath).Returns(PATH);
        A.CallTo(() => _fileSystem.File.Exists(Path.Combine(PATH, FILENAME))).Returns(true);
    }

    [Fact]
    public async Task InitializeAsync_ShouldReadHeidiFile_WhenFileExists()
    {
        var mre = new ManualResetEvent(false);
        A.CallTo(() => _fileSystem.File.Exists(Path.Combine(PATH, FILENAME))).Returns(true);
        A.CallTo(() => _configReader.ReadConfigsAsync(Path.Combine(PATH, FILENAME)))
         .Invokes(() => mre.Set())
         .Returns(Task.FromResult(new Dictionary<string, RepomHeidiConfig>()));

        // act
        await _sut.InitializeAsync();
        mre.WaitOne(TimeSpan.FromSeconds(2));

        // assert
        A.CallTo(() => _fileSystem.FileSystemWatcher.New(A<string>._, A<string>._)).MustHaveHappened();
        A.CallTo(() => _configReader.ReadConfigsAsync(Path.Combine(PATH, FILENAME))).MustHaveHappenedOnceExactly();
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

    [Fact]
    public async Task Events_ShouldNotBeProcessed_WhenWrongFilename()
    {
        var mre = new AutoResetEvent(false);
        A.CallTo(() => _fileSystem.File.Exists(A<string>._)).Returns(true);
        A.CallTo(() => _configReader.ReadConfigsAsync(A<string>._))
         .Invokes(() => mre.Set())
         .Returns(Task.FromResult(new Dictionary<string, RepomHeidiConfig>()));

        await _sut.InitializeAsync();
        mre.WaitOne(TimeSpan.FromSeconds(1));
        Fake.ClearRecordedCalls(_configReader);

        // act
        var evt = new FileSystemEventArgs(WatcherChangeTypes.Changed, PATH, FILENAME + "dummy");
        _changeEventDummyFileSystemWatcher.Change(evt);
        await Task.Delay(5000);

        // assert
        A.CallTo(() => _configReader.ReadConfigsAsync(Path.Combine(PATH, FILENAME))).MustNotHaveHappened();
    }

    [Fact]
    public async Task Events_ShouldBeBundled_WhenHappeningWithinWindow()
    {
        var mre = new AutoResetEvent(false);
        A.CallTo(() => _fileSystem.File.Exists(A<string>._)).Returns(true);
        A.CallTo(() => _configReader.ReadConfigsAsync(A<string>._))
         .Invokes(() => mre.Set())
         .Returns(Task.FromResult(new Dictionary<string, RepomHeidiConfig>()));

        await _sut.InitializeAsync();
        mre.WaitOne(TimeSpan.FromSeconds(1));
        Fake.ClearRecordedCalls(_configReader);

        // act
        var evt = new FileSystemEventArgs(WatcherChangeTypes.Changed, PATH, FILENAME);
        _changeEventDummyFileSystemWatcher.Change(evt);
        await Task.Delay(100);
        _changeEventDummyFileSystemWatcher.Change(evt);
        await Task.Delay(100);
        _changeEventDummyFileSystemWatcher.Change(evt);

        await Task.Delay(5000);

        // assert
        A.CallTo(() => _configReader.ReadConfigsAsync(Path.Combine(PATH, FILENAME))).MustHaveHappenedOnceExactly();
    }
}

internal class ChangeEventDummyFileSystemWatcher : IFileSystemWatcher
{
    public IFileSystem FileSystem { get; }

    public void Dispose()
    {
        return;
    }

    public void BeginInit()
    {
    }

    public void EndInit()
    {
    }

    public IWaitForChangedResult WaitForChanged(WatcherChangeTypes changeType)
    {
        throw new NotImplementedException();
    }

    public IWaitForChangedResult WaitForChanged(WatcherChangeTypes changeType, int timeout)
    {
        throw new NotImplementedException();
    }

    public void Change(FileSystemEventArgs evt)
    {
        Changed?.Invoke(this, evt);
    }

    public IContainer? Container { get; }
    public bool EnableRaisingEvents { get; set; }
    public string Filter { get; set; }
    public Collection<string> Filters { get; }
    public bool IncludeSubdirectories { get; set; }
    public int InternalBufferSize { get; set; }
    public NotifyFilters NotifyFilter { get; set; }
    public string Path { get; set; }
    public ISite? Site { get; set; }
    public ISynchronizeInvoke? SynchronizingObject { get; set; }
    public event FileSystemEventHandler? Changed;
    public event FileSystemEventHandler? Created;
    public event FileSystemEventHandler? Deleted;
    public event ErrorEventHandler? Error;
    public event RenamedEventHandler? Renamed;
}