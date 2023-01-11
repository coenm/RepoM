namespace RepoM.Plugin.Heidi.Tests.Internal;

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
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

[UsesVerify]
public class HeidiConfigurationServiceTests
{
    private const string FILENAME = "heidi.portable.txt";
    private const string PATH = "C:\\heidi\\";

    private readonly IFileSystem _fileSystem;
    private readonly IHeidiPortableConfigReader _configReader;
    private readonly IHeidiSettings _heidiSettings;
    private readonly IFileSystemWatcher _fileWatcher;
    private readonly IRepository _repository;
    private readonly IHeidiRepositoryExtractor _heidiRepositoryExtractor;
    private readonly HeidiConfigurationService _sut;
    private readonly ChangeEventDummyFileSystemWatcher _changeEventDummyFileSystemWatcher;
    private readonly List<HeidiSingleDatabaseConfiguration> _heidiConfigurationResult1;
    private readonly VerifySettings _verifySettings;

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
        A.CallTo(() => _fileSystem.File.Exists(Path.Combine(PATH, FILENAME))).Returns(true);

        A.CallTo(() => _repository.Remotes)
         .Returns(new List<Remote>
            {
                new("bb", "http://github.com/a/b.git"),
                new("origin", "http://github.com/coenm/RepoM.git"),
            });

        _heidiConfigurationResult1 = new List<HeidiSingleDatabaseConfiguration>
            {
                new HeidiSingleDatabaseConfiguration
                    {
                        Key = "OSS/Github/RepoM-P",
                        // Order = 23,
                        // Environment = "Production",
                        // Name = "RepoM Prod",
                        // Repositories = new []{ "RepoM", },
                    },
                new HeidiSingleDatabaseConfiguration
                    {
                        Key = "OSS/Github/RepoM-D",
                        // Order = 5,
                        // Environment = "Development",
                        // Name = "RepoM Dev!!",
                        // Repositories = new []{ "RepoM", "RepomTest", },
                    },
                new HeidiSingleDatabaseConfiguration
                    {
                        Key = "OSS/Abc",
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

            yield return new object[] { l, fs, hpcr, hre, null!, };
            yield return new object[] { l, fs, hpcr, null!, hs, };
            yield return new object[] { l, fs, null!, hre, hs, };
            yield return new object[] { l, null!, hpcr, hre, hs, };
            yield return new object[] { null!, fs, hpcr, hre, hs, };
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
    public async Task GetByRepository_ShouldReturnDatabasesForSpecificRepository_WhenInitializationIsFinished()
    {
        // arrange
        var mre = new ManualResetEvent(false);
        _sut.ConfigurationUpdated += (_, _) => mre.Set();
        A.CallTo(() => _fileSystem.File.Exists(Path.Combine(PATH, FILENAME))).Returns(true);
        A.CallTo(() => _configReader.ParseAsync(Path.Combine(PATH, FILENAME)))
         .Returns(Task.FromResult(_heidiConfigurationResult1));

        RepoHeidi? aRef;
        A.CallTo(() => _heidiRepositoryExtractor.TryExtract(A<HeidiSingleDatabaseConfiguration>._, out aRef))
         .Returns(true)
         .AssignsOutAndRefParametersLazily((HeidiSingleDatabaseConfiguration x, RepoHeidi? someRef) =>
             new object[] { new RepoHeidi(), });

        await _sut.InitializeAsync();
        mre.WaitOne(TimeSpan.FromSeconds(2));

        // act
        IEnumerable<HeidiConfiguration> result = _sut.GetByRepository(_repository);
        
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
         .Returns(new List<Remote>
             {
                 new("bb", "http://github.com/a/b.git"),
                 new(originKey, "http://github.com/coenm/Abcd.git"),
             });

        var mre = new ManualResetEvent(false);
        _sut.ConfigurationUpdated += (_, _) => mre.Set();
        A.CallTo(() => _fileSystem.File.Exists(Path.Combine(PATH, FILENAME))).Returns(true);
        A.CallTo(() => _configReader.ParseAsync(Path.Combine(PATH, FILENAME)))
         .Returns(Task.FromResult(_heidiConfigurationResult1));

        await _sut.InitializeAsync();
        mre.WaitOne(TimeSpan.FromSeconds(2));

        // act
        IEnumerable<HeidiConfiguration> result = _sut.GetByRepository(_repository);
        
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
        A.CallTo(() => _fileSystem.File.Exists(Path.Combine(PATH, FILENAME))).Returns(true);
        A.CallTo(() => _configReader.ParseAsync(Path.Combine(PATH, FILENAME)))
         .Returns(Task.FromResult(_heidiConfigurationResult1));

        await _sut.InitializeAsync();
        mre.WaitOne(TimeSpan.FromSeconds(2));

        // act
        IEnumerable<HeidiConfiguration> result = _sut.GetByKey(key!);

        // assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task InitializeAsync_ShouldReadHeidiFile_WhenFileExists()
    {
        var mre = new ManualResetEvent(false);
        _sut.ConfigurationUpdated += (_, _) => mre.Set();
        A.CallTo(() => _fileSystem.File.Exists(Path.Combine(PATH, FILENAME))).Returns(true);
        A.CallTo(() => _configReader.ParseAsync(Path.Combine(PATH, FILENAME)))
         .Returns(Task.FromResult(_heidiConfigurationResult1));

        // act
        await _sut.InitializeAsync();
        mre.WaitOne(TimeSpan.FromSeconds(2));

        // assert
        A.CallTo(() => _fileSystem.FileSystemWatcher.New(A<string>._, A<string>._)).MustHaveHappened();
        A.CallTo(() => _configReader.ParseAsync(Path.Combine(PATH, FILENAME))).MustHaveHappenedOnceExactly();
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
        await Task.Delay(5000);

        // assert
        A.CallTo(() => _configReader.ParseAsync(Path.Combine(PATH, FILENAME))).MustNotHaveHappened();
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
             Task.FromResult(_heidiConfigurationResult1));

        await _sut.InitializeAsync();
        mre.WaitOne(TimeSpan.FromSeconds(1));
        Fake.ClearRecordedCalls(_configReader);

        // act
        _changeEventDummyFileSystemWatcher.Change(PATH, FILENAME);
        await Task.Delay(100);
        _changeEventDummyFileSystemWatcher.Change(PATH, FILENAME);
        await Task.Delay(100);
        _changeEventDummyFileSystemWatcher.Change(PATH, FILENAME);

        await Task.Delay(5000);

        // assert
        A.CallTo(() => _configReader.ParseAsync(Path.Combine(PATH, FILENAME))).MustHaveHappenedOnceExactly();
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
        mre.WaitOne(TimeSpan.FromSeconds(1));
        
        Fake.ClearRecordedCalls(_configReader);

        // act
        _sut.Dispose();

        _changeEventDummyFileSystemWatcher.Change(PATH, FILENAME);
        await Task.Delay(5000);

        // assert
        A.CallTo(() => _configReader.ParseAsync(Path.Combine(PATH, FILENAME))).MustNotHaveHappened();
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