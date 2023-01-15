namespace RepoM.Plugin.WindowsExplorerGitInfo.Tests;

using System;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using System.Threading.Tasks;
using FakeItEasy;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using RepoM.Core.Plugin.Common;
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
}