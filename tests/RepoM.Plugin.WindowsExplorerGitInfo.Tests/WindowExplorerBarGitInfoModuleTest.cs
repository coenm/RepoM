namespace RepoM.Plugin.WindowsExplorerGitInfo.Tests;

using System;
using System.Threading;
using System.Threading.Tasks;
using FakeItEasy;
using AwesomeAssertions;
using RepoM.Plugin.WindowsExplorerGitInfo.PInvoke.Explorer;
using Xunit;

public class WindowExplorerBarGitInfoModuleTest
{
    private readonly IWindowsExplorerHandler _explorerHandler;
    private readonly WindowExplorerBarGitInfoModule _sut;

    public WindowExplorerBarGitInfoModuleTest()
    {
        _explorerHandler = A.Fake<IWindowsExplorerHandler>();
        _sut = new WindowExplorerBarGitInfoModule(_explorerHandler);
    }

    [Fact]
    public void Ctor_ShouldThrow_WhenArgumentIsNull()
    {
        // arrange

        // act
        Action act = () => _ = new WindowExplorerBarGitInfoModule(null!);

        // assert
        act.Should().ThrowExactly<ArgumentNullException>();
    }

    [Fact]
    public async Task StopAsync_ShouldCleanTitles()
    {
        // arrange

        // act
        await _sut.StopAsync();

        // assert
        A.CallTo(() => _explorerHandler.CleanTitles()).MustHaveHappenedOnceExactly();
    }

    // fragile?
    [Fact]
    public async Task StartAsync_ShouldCallUpdateTitlesUntilStopped()
    {
        // arrange
        var count = 0;
        var mre = new ManualResetEvent(false);
        A.CallTo(() => _explorerHandler.UpdateTitles()).
          Invokes(_ =>
            {
                var currentCount = Interlocked.Increment(ref count);
                if (currentCount == 3)
                {
                    mre.Set();
                }
            });
    
        // act
        await _sut.StartAsync();
        var set = mre.WaitOne(TimeSpan.FromSeconds(15));
    
        // assert
        set.Should().BeTrue();
        A.CallTo(() => _explorerHandler.UpdateTitles()).MustHaveHappened(3, Times.Exactly);
        A.CallTo(() => _explorerHandler.CleanTitles()).MustNotHaveHappened();
    }

    // fragile?
    [Fact]
    public async Task StopAsync_ShouldCancelTimerExecution_WhenStarted()
    {
        // arrange
        var count = 0;
        var mreAfterStart = new ManualResetEvent(false);
        var mreAfterStop = new ManualResetEvent(false);
        A.CallTo(() => _explorerHandler.UpdateTitles()).
          Invokes(_ =>
              {
                  var currentCount = Interlocked.Increment(ref count);
                  if (currentCount == 2)
                  {
                      mreAfterStart.Set();
                      return;
                  }

                  if (currentCount > 2)
                  {
                      mreAfterStop.Set();
                  }
              });

        await _sut.StartAsync();
        var mreAfterStartSet = mreAfterStart.WaitOne(TimeSpan.FromSeconds(15));

        // act
        await _sut.StopAsync();
        var mreAfterStopSet  = mreAfterStop.WaitOne(TimeSpan.FromSeconds(10));

        // assert
        mreAfterStartSet.Should().BeTrue();
        mreAfterStopSet.Should().BeFalse(); 
        A.CallTo(() => _explorerHandler.UpdateTitles()).MustHaveHappenedTwiceOrMore();
        A.CallTo(() => _explorerHandler.CleanTitles()).MustHaveHappenedOnceExactly();
    }
}