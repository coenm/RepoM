namespace RepoM.Plugin.Heidi.Tests.Internal;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FakeItEasy;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using RepoM.Core.Plugin.Repository;
using RepoM.Plugin.Heidi.Interface;
using RepoM.Plugin.Heidi.Internal;
using RepoM.Plugin.Heidi.Internal.Config;
using VerifyTests;
using VerifyXunit;
using Xunit;

public class HeidiConfigurationServiceTests
{
    private const string FILENAME = "heidi.portable.txt";
    private const string PATH = "C:\\heidi\\";
    private readonly string _fullFilename = Path.Combine(PATH, FILENAME);
    private readonly TimeSpan _initTimeout = TimeSpan.FromSeconds(30);

    private readonly IFileSystem _fileSystem;
    private readonly IHeidiPortableConfigReader _configReader;
    private readonly IHeidiSettings _heidiSettings;
    private readonly IFileSystemWatcher _fileWatcher;
    private readonly IRepository _repository;
    private readonly IHeidiRepositoryExtractor _heidiRepositoryExtractor;
    private readonly HeidiConfigurationService _sut;
    private readonly ChangeEventDummyFileSystemWatcher _changeEventDummyFileSystemWatcher;
    private readonly List<HeidiSingleDatabaseConfiguration> _heidiConfigurationResult;
    private readonly VerifySettings _verifySettings;
    private readonly List<RepoHeidi> _heidis;

    public HeidiConfigurationServiceTests()
    {
        _verifySettings = new VerifySettings();
        _verifySettings.UseDirectory("Verified");

        _changeEventDummyFileSystemWatcher = new ChangeEventDummyFileSystemWatcher();
        _fileWatcher = A.Fake<IFileSystemWatcher>(o => o.Wrapping(_changeEventDummyFileSystemWatcher));

        _fileSystem = A.Fake<IFileSystem>();
        _configReader = A.Fake<IHeidiPortableConfigReader>();
        _heidiSettings = A.Fake<IHeidiSettings>();
        _repository = A.Fake<IRepository>();
        _heidiRepositoryExtractor = A.Fake<IHeidiRepositoryExtractor>();
        _sut = new HeidiConfigurationService(NullLogger.Instance, _fileSystem, _configReader, _heidiRepositoryExtractor, _heidiSettings);

        A.CallTo(() => _fileSystem.FileSystemWatcher.New(A<string>._, A<string>._)).Returns(_fileWatcher);
        A.CallTo(() => _heidiSettings.ConfigFilename).Returns(FILENAME);
        A.CallTo(() => _heidiSettings.ConfigPath).Returns(PATH);
        A.CallTo(() => _fileSystem.File.Exists(_fullFilename)).Returns(true);

        A.CallTo(() => _repository.Remotes)
         .Returns(new List<Remote>
            {
                new("bb", "http://github.com/a/b.git"),
                new("origin", "http://github.com/coenm/RepoM.git"),
            });

        _heidis = new List<RepoHeidi>
            {
                new RepoHeidi("OSS/Github/RepoM-P")
                    {
                        Name = "RepoM Prod",
                        Order = 23,
                        Tags = [],
                        Repository = "RepoM",
                    },
                new RepoHeidi("OSS/Github/RepoM-D")                    {
                        Name = "RepoM Dev!!",
                        Order = 5,
                        Tags = [],
                        Repository = "RepoM",
                    },
                new RepoHeidi("OSS/Abc")                    {
                        Name = "Abc Prod",
                        Order = 0,
                        Tags = [],
                        Repository = "Abc",
                    },
            };

        _heidiConfigurationResult = new List<HeidiSingleDatabaseConfiguration>
            {
                new HeidiSingleDatabaseConfiguration("OSS/Github/RepoM-P")
                    {
                        // Order = 23,
                        // Environment = "Production",
                        // Name = "RepoM Prod",
                        // Repositories = new []{ "RepoM", },
                    },
                new HeidiSingleDatabaseConfiguration("OSS/Github/RepoM-D")
                    {
                        // Order = 5,
                        // Environment = "Development",
                        // Name = "RepoM Dev!!",
                        // Repositories = new []{ "RepoM", "RepomTest", },
                    },
                new HeidiSingleDatabaseConfiguration("OSS/Abc")
                    {
                        // Order = 0,
                        // Environment = "Production",
                        // Name = "Abc Prod",
                        // Repositories = new []{ "Abc", },
                    },
            };
    }

    public static IEnumerable<object[]> CtorNullArguments
    {
        get
        {
            var l = A.Dummy<ILogger>();
            var fs = A.Dummy<IFileSystem>();
            var hpcr = A.Dummy<IHeidiPortableConfigReader>();
            var hre = A.Dummy<IHeidiRepositoryExtractor>();
            var hs = A.Dummy<IHeidiSettings>();

            yield return [l, fs, hpcr, hre, null!,];
            yield return [l, fs, hpcr, null!, hs,];
            yield return [l, fs, null!, hre, hs,];
            yield return [l, null!, hpcr, hre, hs,];
            yield return [null!, fs, hpcr, hre, hs,];
        }
    }

    [Theory]
    [MemberData(nameof(CtorNullArguments))]
    internal void Ctor_ShouldThrow_WhenArgumentIsNull(
        ILogger? logger,
        IFileSystem? fileSystem,
        IHeidiPortableConfigReader? reader,
        IHeidiRepositoryExtractor? heidiRepositoryExtractor,
        IHeidiSettings? heidiSettings)
    {
        // arrange

        // act
        Action act = () => _ = new HeidiConfigurationService(logger!, fileSystem!, reader!, heidiRepositoryExtractor!, heidiSettings!);

        // assert
        act.Should().ThrowExactly<ArgumentNullException>();
    }

    [Fact]
    public async Task GetAllDatabases_ShouldReturnDatabases_WhenInitializedCompleted()
    {
        // arrange
        A.CallTo(() => _configReader.ParseAsync(_fullFilename))
         .Returns(Task.FromResult(_heidiConfigurationResult));
        var mre = new ManualResetEvent(false);
        _sut.ConfigurationUpdated += (_, _) => mre.Set();

        await _sut.InitializeAsync();
        mre.WaitOne(_initTimeout);

        // act
        ImmutableArray<HeidiSingleDatabaseConfiguration> result = _sut.GetAllDatabases();
        
        // assert
        result.Should().BeEquivalentTo(_heidiConfigurationResult);
    }
    
    [Fact]
    public async Task GetByRepository_ShouldReturnDatabasesForSpecificRepository_WhenInitializationIsFinished()
    {
        // arrange
        var mre = new ManualResetEvent(false);
        _sut.ConfigurationUpdated += (_, _) => mre.Set();
        A.CallTo(() => _fileSystem.File.Exists(_fullFilename)).Returns(true);
        A.CallTo(() => _configReader.ParseAsync(_fullFilename))
         .Returns(Task.FromResult(_heidiConfigurationResult));

        RepoHeidi? aRef;
        A.CallTo(() => _heidiRepositoryExtractor.TryExtract(A<HeidiSingleDatabaseConfiguration>._, out aRef))
         .ReturnsLazily(call =>
             {
                 if (call.Arguments.First() is HeidiSingleDatabaseConfiguration h)
                 {
                     return _heidis.Any(x => x.HeidiKey.Equals(h.Key));
                 }

                 throw new Exception("thrown by test");
             })
         .AssignsOutAndRefParametersLazily((HeidiSingleDatabaseConfiguration config, RepoHeidi? output) =>
             {
                 RepoHeidi[] matches = _heidis.Where(x => x.HeidiKey.Equals(config.Key)).ToArray();
                 if (matches.Length > 0)
                 {
                     return [matches.First(),];
                 }
                 return [null!,];

             });

        await _sut.InitializeAsync();
        mre.WaitOne(_initTimeout);

        // act
        IEnumerable<RepositoryHeidiConfiguration> result = _sut.GetByRepository(_repository);
        
        // assert
        _ = await Verifier.Verify(result, _verifySettings);
    }

    [Theory]
    [InlineData("origin")]
    [InlineData("cc")]
    public async Task GetByRepository_ShouldReturnEmpty_WhenInitializationIsFinishedButNothingFoundForRepository(string originKey)
    {
        // arrange
        A.CallTo(() => _repository.Remotes)
         .Returns(
             [
                 new("bb", "http://github.com/a/b.git"),
                 new(originKey, "http://github.com/coenm/Abcd.git"),
             ]);

        var mre = new ManualResetEvent(false);
        _sut.ConfigurationUpdated += (_, _) => mre.Set();
        A.CallTo(() => _fileSystem.File.Exists(_fullFilename)).Returns(true);
        A.CallTo(() => _configReader.ParseAsync(_fullFilename))
         .Returns(Task.FromResult(_heidiConfigurationResult));

        await _sut.InitializeAsync();
        mre.WaitOne(_initTimeout);

        // act
        IEnumerable<RepositoryHeidiConfiguration> result = _sut.GetByRepository(_repository);
        
        // assert
        result.Should().BeEmpty();
    }


    [Theory]
    [InlineData(" ")]
    [InlineData("")]
    [InlineData(null)]
    public async Task GetByKey_ShouldReturnEmpty_WhenKeyIsNullOrEmpty(string? key)
    {
        // arrange
        var mre = new ManualResetEvent(false);
        _sut.ConfigurationUpdated += (_, _) => mre.Set();
        A.CallTo(() => _fileSystem.File.Exists(_fullFilename)).Returns(true);
        A.CallTo(() => _configReader.ParseAsync(_fullFilename))
         .Returns(Task.FromResult(_heidiConfigurationResult));

        await _sut.InitializeAsync();
        mre.WaitOne(_initTimeout);

        // act
        IEnumerable<RepositoryHeidiConfiguration> result = _sut.GetByKey(key!);

        // assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task InitializeAsync_ShouldReadHeidiFile_WhenFileExists()
    {
        var mre = new ManualResetEvent(false);
        _sut.ConfigurationUpdated += (_, _) => mre.Set();
        A.CallTo(() => _fileSystem.File.Exists(_fullFilename)).Returns(true);
        A.CallTo(() => _configReader.ParseAsync(_fullFilename))
         .Returns(Task.FromResult(_heidiConfigurationResult));

        // act
        await _sut.InitializeAsync();
        var updated = mre.WaitOne(_initTimeout);

        // assert
        updated.Should().BeTrue("we expect an event to happen.");
        A.CallTo(() => _fileSystem.FileSystemWatcher.New(A<string>._, A<string>._)).MustHaveHappened();
        A.CallTo(() => _configReader.ParseAsync(_fullFilename)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task InitializeAsync_ShouldNotReadHeidiFile_WhenFileNotExists()
    {
        // arrange
        A.CallTo(() => _fileSystem.File.Exists(A<string>._)).Returns(false);

        // act
        await _sut.InitializeAsync();

        // assert
        A.CallTo(() => _fileSystem.FileSystemWatcher.New(A<string>._, A<string>._)).MustNotHaveHappened();
        A.CallTo(() => _configReader.ParseAsync(A<string>._)).MustNotHaveHappened();
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
        _sut.ConfigurationUpdated += (_, _) => mre.Set();
        A.CallTo(() => _fileSystem.File.Exists(A<string>._)).Returns(true);
        A.CallTo(() => _configReader.ParseAsync(A<string>._))
         .Returns(Task.FromResult(new List<HeidiSingleDatabaseConfiguration>()));

        await _sut.InitializeAsync();
        mre.WaitOne(TimeSpan.FromSeconds(1));
        Fake.ClearRecordedCalls(_configReader);

        // act
        _changeEventDummyFileSystemWatcher.Change(PATH, FILENAME + "dummy");
        await Task.Delay(TimeSpan.FromSeconds(10));

        // assert
        A.CallTo(() => _configReader.ParseAsync(_fullFilename)).MustNotHaveHappened();
    }

    [Fact]
    public async Task Events_ShouldBeBundled_WhenHappeningWithinWindow()
    {
        var mre = new AutoResetEvent(false);
        _sut.ConfigurationUpdated += (_, _) => mre.Set();
        A.CallTo(() => _fileSystem.File.Exists(A<string>._)).Returns(true);
        A.CallTo(() => _configReader.ParseAsync(A<string>._))
         .ReturnsNextFromSequence(
             Task.FromResult(new List<HeidiSingleDatabaseConfiguration>()),
             Task.FromResult(_heidiConfigurationResult));

        await _sut.InitializeAsync();
        mre.WaitOne(_initTimeout);
        Fake.ClearRecordedCalls(_configReader);

        // act
        _changeEventDummyFileSystemWatcher.Change(PATH, FILENAME);
        await Task.Delay(100);
        _changeEventDummyFileSystemWatcher.Change(PATH, FILENAME);
        await Task.Delay(100);
        _changeEventDummyFileSystemWatcher.Change(PATH, FILENAME);

        await Task.Delay(TimeSpan.FromSeconds(10));

        // assert
        A.CallTo(() => _configReader.ParseAsync(_fullFilename)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Dispose_ShouldRemoveSubscription()
    {
        var mre = new AutoResetEvent(false);
        _sut.ConfigurationUpdated += (_, _) => mre.Set();
        A.CallTo(() => _fileSystem.File.Exists(A<string>._)).Returns(true);
        A.CallTo(() => _configReader.ParseAsync(A<string>._))
         .Returns(Task.FromResult(new List<HeidiSingleDatabaseConfiguration>()));

        await _sut.InitializeAsync();
        mre.WaitOne(_initTimeout);
        
        Fake.ClearRecordedCalls(_configReader);

        // act
        _sut.Dispose();

        _changeEventDummyFileSystemWatcher.Change(PATH, FILENAME);
        await Task.Delay(TimeSpan.FromSeconds(10));

        // assert
        A.CallTo(() => _configReader.ParseAsync(_fullFilename)).MustNotHaveHappened();
    }

    [Fact]
    public void Dispose_ShouldDoNothing_WhenNotInitialized()
    {
        // arrange

        // act
        Action act = () => _sut.Dispose();

        // assert
        act.Should().NotThrow();
    }
}