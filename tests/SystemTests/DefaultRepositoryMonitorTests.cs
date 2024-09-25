namespace SystemTests;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FakeItEasy;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using RepoM.Api.Git;
using RepoM.Api.Git.AutoFetch;
using RepoM.Api.IO;
using RepoM.Api.IO.ModuleBasedRepositoryActionProvider;
using SystemTests.IO;
using SystemTests.Mocks;

public class DefaultRepositoryMonitorTests
{
    private static readonly IFileSystem _fileSystem = new FileSystem();
    private static RepositoryWriter _origin = null!;
    private static RepositoryWriter _cloneA = null!;
    private static RepositoryWriter _cloneB = null!;
    private static DefaultRepositoryMonitor _monitor = null!;
    private static string _rootPath = string.Empty;
    private static string _defaultBranch = "master";

    [Before(Class)]
    public static void OneTimeSetUp()
    {
        var fileSystem = new FileSystem();
        _rootPath = Path.Combine(Path.GetTempPath(), "RepoM_Test_Repositories");

        TryCreateRootPath(_rootPath);

        var repoPath = Path.Combine(_rootPath, Guid.NewGuid().ToString());
        _fileSystem.Directory.CreateDirectory(repoPath);

        IFileSystem fs = new FileSystem();
        var defaultRepositoryReader = new DefaultRepositoryReader(A.Dummy<IRepositoryTagsFactory>(), NullLogger.Instance);
        _monitor = new DefaultRepositoryMonitor(
            new GivenPathProvider([repoPath,]),
            defaultRepositoryReader,
            new DefaultRepositoryDetectorFactory(defaultRepositoryReader, fs, NullLoggerFactory.Instance),
            new DefaultRepositoryObserverFactory(NullLoggerFactory.Instance, fs),
            new GitRepositoryFinderFactory(new GravellGitRepositoryFinderFactory(new NeverSkippingPathSkipper(), _fileSystem)),
            new UselessRepositoryStore(),
            new DefaultRepositoryInformationAggregator(
                new DirectThreadDispatcher()),
            A.Dummy<IAutoFetchHandler>(),
            A.Dummy<IRepositoryIgnoreStore>(),
            _fileSystem,
            NullLogger.Instance)
            {
                DelayGitRepositoryStatusAfterCreationMilliseconds = 100,
                DelayGitStatusAfterFileOperationMilliseconds = 100,
            };

        _origin = new RepositoryWriter(Path.Combine(repoPath, "BareOrigin"), fileSystem);
        _cloneA = new RepositoryWriter(Path.Combine(repoPath, "CloneA"), fileSystem);
        _cloneB = new RepositoryWriter(Path.Combine(repoPath, "CloneB"), fileSystem);
    }

    [After(Class)]
    public static void TearDown()
    {
        _monitor.Stop();
        //_monitor.Dispose();

        WaitFileOperationDelay();

        TryDeleteRootPath(_rootPath);
    }

    /*

             [[1]]                          [[1]]
cloneA   <-----------------+   origin  +----------------->  cloneB
 +                                 +                        +
 |                           ^  ^  |                        |
 v       [[2]]               |  |  |         [[3]]          v
add file                        |  |  +----------------------> +--+    [[4]]
 +                           |  |  |        pull            |  |
 |                           |  |  |                        |  |
 v                           |  |  |                        |  +---->  branch "develop"
stage file                      |  |  |fetch                   |              |
 +                           |  |  |                  master|              v
 |                           |  |  +--> +                   |          create file
 v                  push to  |  |       |                   |              |
commit file             master   |  |       |                   |              v
 +---------------------------^  |       |                   |          stage file
                                |       |                   |              |
                                |       |                   |              v
                                |       |     [[5]]         |          commit file
                                |       |            merge  v   [[6]]      |
      [[9]]                     |       +-----------------> @------------->+
  delete cloneA                 |                           |    rebase    |
                                |                           |              |
                                |                           +<-------------+
                                |                           |   merge
                                |                           |   [[7]]
                                |    push                   |
                                +---------------------------+
                                         [[8]]

     */

    [Test]
    public void T0A_Detects_Repository_Creation()
    {
        _monitor.Expect(() => _origin.InitBare(),
            changes => changes == 0,
            deletes => deletes == 0);
    }

    [Test]
    [DependsOn(nameof(T0A_Detects_Repository_Creation))]
    public async Task T1A_Detects_Repository_Clone()
    {
        _monitor.Expect(() =>
                {
                    _cloneA.Clone(_origin.Path);
                    _cloneB.Clone(_origin.Path);
                },
            changes => changes >= 0, /* TODO */
            deletes => deletes == 0);

        _defaultBranch = _cloneA.CurrentBranch;
        await Assert.That(_cloneB.CurrentBranch).IsEqualTo(_defaultBranch);
    }
    
    [Test]
    [DependsOn(nameof(T1A_Detects_Repository_Clone))]
    public void T2B_Detects_File_Creation()
    {
        _monitor.Expect(() => _cloneA.CreateFile("First.A", "First file on clone A"),
            changes => changes >= 0, /* TODO */
            deletes => deletes == 0);
    }
    
    [Test]
    [DependsOn(nameof(T2B_Detects_File_Creation))]
    public void T2C_Detects_File_Staging()
    {
        _monitor.Expect(() => _cloneA.Stage("First.A"),
            changes => changes >= 0, /* TODO */
            deletes => deletes == 0);
    }
    
    [Test]
    [DependsOn(nameof(T2C_Detects_File_Staging))]
    public void T2D_Detects_Repository_Commits()
    {
        _monitor.Expect(() => _cloneA.Commit("Commit #1 on A"),
            changes => changes >= 1,
            deletes => deletes == 0);
    }
    
    [Test]
    [DependsOn(nameof(T2D_Detects_Repository_Commits))]
    public void T2E_Detects_Repository_Pushes()
    {
        _monitor.Expect(() =>
                {
                    _cloneA.Push();
                    _origin.HeadTip.Should().Be(_cloneA.HeadTip);
                },
            changes => changes >= 1,
            deletes => deletes == 0);
    }
    
    [Test]
    [DependsOn(nameof(T2E_Detects_Repository_Pushes))]
    public void T3A_Detects_Repository_Pull()
    {
        _monitor.Expect(() =>
                {
                    _cloneB.Pull();
                    _cloneB.HeadTip.Should().Be(_cloneA.HeadTip);
                },
            changes => changes >= 1,
            deletes => deletes == 0);
    }
    
    [Test]
    [DependsOn(nameof(T3A_Detects_Repository_Pull))]
    public void T4A_Detects_Repository_Branch_And_Checkout()
    {
        _monitor.Expect(() =>
                {
                        
                    _cloneB.CurrentBranch.Should().BeOneOf(_defaultBranch);
                    _cloneB.Branch("develop");
                    _cloneB.Checkout("develop");
                    _cloneB.CurrentBranch.Should().Be("develop");
                },
            changes => changes >= 1,
            deletes => deletes == 0);
    }
    
    [Test]
    [DependsOn(nameof(T4A_Detects_Repository_Branch_And_Checkout))]
    public void T4B_Preparation_Add_Changes_To_A_And_Push()
    {
        _cloneA.CreateFile("Second.A", "Second file on clone A");
        _cloneA.Stage("Second.A");
        _cloneA.Commit("Commit #2 on A");
        _cloneA.Push();
    }
    
    [Test]
    [DependsOn(nameof(T4B_Preparation_Add_Changes_To_A_And_Push))]
    public void T4C_Preparation_Add_Changes_To_B()
    {
        _cloneB.CreateFile("First.B", "First file on clone B");
        _cloneB.Stage("First.B");
        _cloneB.Commit("Commit #1 on B");
    }
    
    [Test]
    [DependsOn(nameof(T4C_Preparation_Add_Changes_To_B))]
    public void T5A_Preparation_Checkout_Master()
    {
        _cloneB.CurrentBranch.Should().Be("develop");
        _cloneB.Checkout(_defaultBranch);
        _cloneB.CurrentBranch.Should().Be(_defaultBranch);
    }
    
    [Test]
    [DependsOn(nameof(T5A_Preparation_Checkout_Master))]
    public void T5B_Detects_Repository_Fetch()
    {
        _monitor.Expect(() => _cloneB.Fetch(),
            changes => changes >= 1,
            deletes => deletes == 0);
    }
    
    [Test]
    [DependsOn(nameof(T5B_Detects_Repository_Fetch))]
    public void T5C_Detects_Repository_Merge_Tracked_Branch()
    {
        _monitor.Expect(() => _cloneB.MergeWithTracked(),
            changes => changes >= 1,
            deletes => deletes == 0);
    }
    
    [Test]
    [DependsOn(nameof(T5C_Detects_Repository_Merge_Tracked_Branch))]
    public void T6A_Preparation_Checkout_Develop()
    {
        _cloneB.CurrentBranch.Should().Be(_defaultBranch);
        _cloneB.Checkout("develop");
        _cloneB.CurrentBranch.Should().Be("develop");
    }
    
    [Test]
    [DependsOn(nameof(T6A_Preparation_Checkout_Develop))]
    public void T6B_Detects_Repository_Rebase()
    {
        _monitor.Expect(() =>
                {
                    var steps = _cloneB.Rebase(_defaultBranch);
                    steps.Should().Be(1);
                },
            changes => changes >= 1,
            deletes => deletes == 0);
    }
    
    [Test]
    [DependsOn(nameof(T6B_Detects_Repository_Rebase))]
    public void T7A_Preparation_Checkout_Master()
    {
        _cloneB.CurrentBranch.Should().Be("develop");
        _cloneB.Checkout(_defaultBranch);
        _cloneB.CurrentBranch.Should().Be(_defaultBranch);
    }
    
    [Test]
    [DependsOn(nameof(T7A_Preparation_Checkout_Master))]
    public void T7B_Detects_Repository_Merge_With_Other_Branch()
    {
        _monitor.Expect(() => _cloneB.Merge("develop"),
            changes => changes >= 1,
            deletes => deletes == 0);
    }
    
    [Test]
    [DependsOn(nameof(T7B_Detects_Repository_Merge_With_Other_Branch))]
    public void T8A_Detects_Repository_Push_With_Upstream()
    {
        _monitor.Expect(
            () =>
                {
                    _origin.HeadTip.Should().NotBe(_cloneB.HeadTip);
                    _cloneB.Push();
                    _origin.HeadTip.Should().Be(_cloneB.HeadTip);
                },
            changes => changes >= 1,
            deletes => deletes == 0);
    }
    
    [Test]
    [DependsOn(nameof(T8A_Detects_Repository_Push_With_Upstream))]
    public void T9A_Detects_Repository_Deletion()
    {
        NormalizeReadOnlyFiles(_cloneA.Path);
    
        _monitor.Expect(
            () => _fileSystem.Directory.Delete(_cloneA.Path, true),
            changes: 0,
            deletes: 1);
    }

    private static void TryDeleteRootPath(string rootPath)
    {
        if (!_fileSystem.Directory.Exists(rootPath))
        {
            return;
        }

        WaitFileOperationDelay();

        try
        {
            NormalizeReadOnlyFiles(rootPath);

            _fileSystem.Directory.Delete(rootPath, true);
        }
        catch (UnauthorizedAccessException)
        {
            // we cannot do nothing about it here
            Debug.WriteLine(nameof(UnauthorizedAccessException) + ": Could not clear test root path: " + rootPath);
        }

        WaitFileOperationDelay();
    }

    private static void NormalizeReadOnlyFiles(string rootPath)
    {
        // set readonly git files to "normal" 
        // otherwise we get UnauthorizedAccessExceptions
        IEnumerable<string> readOnlyFiles = _fileSystem.Directory
           .GetFiles(rootPath, "*.*", SearchOption.AllDirectories)
           .Where(f => File.GetAttributes(f).HasFlag(FileAttributes.ReadOnly));

        foreach (var file in readOnlyFiles)
        {
            File.SetAttributes(file, FileAttributes.Normal);
        }
    }

    private static void TryCreateRootPath(string rootPath)
    {
        TryDeleteRootPath(rootPath);

        if (_fileSystem.Directory.Exists(rootPath))
        {
            return;
        }

        _fileSystem.Directory.CreateDirectory(rootPath);
    }

    private static void WaitFileOperationDelay()
    {
        Thread.Sleep(500);
    }
}