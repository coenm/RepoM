namespace RepoM.Plugin.WindowsExplorerGitInfo.Tests;

using System;
using System.Threading;
using System.Threading.Tasks;
using FakeItEasy;
using FluentAssertions;
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
        _ = mre.WaitOne(TimeSpan.FromSeconds(5));
    
        // assert
        A.CallTo(() => _explorerHandler.UpdateTitles()).MustHaveHappened(3, Times.Exactly);
        A.CallTo(() => _explorerHandler.CleanTitles()).MustNotHaveHappened();
    }

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
        _ = mreAfterStart.WaitOne(TimeSpan.FromSeconds(2));

        // act
        await _sut.StopAsync();
        _ = mreAfterStop.WaitOne(TimeSpan.FromSeconds(2));

        // assert
        A.CallTo(() => _explorerHandler.UpdateTitles()).MustHaveHappened(2, Times.Exactly);
        A.CallTo(() => _explorerHandler.CleanTitles()).MustHaveHappenedOnceExactly();
    }
}