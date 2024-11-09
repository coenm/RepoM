namespace UiTests;

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Threading;
using System.Threading.Tasks;
using FakeItEasy;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Input;
using FlaUI.Core.WindowsAPI;
using FlaUI.UIA3;
using FluentAssertions;
using global::RepoM.Api.Common;
using global::RepoM.Api.Git;
using global::RepoM.Core.Plugin.Common;
using Microsoft.Extensions.Logging.Abstractions;
using UiTests.Extensions;
using UiTests.Framework;
using UiTests.RepoM;
using UiTests.Utils;
using UiTests.VisualStudioCode;
using UiTests.VisualStudioCode.WebSockets.Commands;
using Xunit;
using Xunit.Abstractions;
using Path = System.IO.Path;

//C:\Projects\RepoM-Git-repos

[SetTestName]
public class NotePadTest
{
    private readonly ITestOutputHelper _outputHelper;

    private readonly VisualStudioCodeApp _vsCodeApp;
    private readonly UIA3Automation _automation;
    private readonly Uri _visualStudioWebSocketAddress = new("ws://localhost:6783");

    public NotePadTest(ITestOutputHelper outputHelper)
    {
        _outputHelper = outputHelper ?? throw new ArgumentNullException(nameof(outputHelper));

        _automation = new UIA3Automation();
        var vsCodeAppLauncher = new VisualStudioCodeAppLauncher(
            ApplicationFactory.VS_CODE_EXE,
            _automation,
            _outputHelper,
            _visualStudioWebSocketAddress);
        _vsCodeApp = vsCodeAppLauncher.Launch();
    }

    // make sure no vs code instance is running.


    [Fact]
    public async Task OpenWindowInVsCode()
    {
        CancellationToken ct = CancellationToken.None;
        await Task.Delay(10_000);
        _vsCodeApp.WaitForWindow();

        // using var ws = new VisualStudioWebSocketAutomation(_visualStudioWS, _outputHelper);
        // await ws.ConnectAsync(new CancellationTokenSource(TimeSpan.FromSeconds(20)).Token);
        // ws.StartProcessing(ct);

        await Task.Delay(1000, ct);
        var result = "";

        var basePath = Path.GetTempPath();
        var path = Path.Combine(basePath, "RepoM-UI-testing", DateTime.UtcNow.ToString("yyyyMMdd-HHmmss"));
        InitRepoM(path);
        using var clearDirectory = ClearDirectoryOnDisposal(path);
        _vsCodeApp.OpenFile(System.IO.Path.Combine(path, "RepositoryActionsV2.yaml"));



        // result = await ws.ExecuteCommandAsync(new OpenFileVscCommand("~\\OpenId.txt"));
        // result = await _vsCodeApp.ExecuteCommand(new OpenFileVscCommand("cdtmp/gen.md"));
        // await Task.Delay(4000, ct);

        // result = await ws.ExecuteCommandAsync(CommandTypes.WorkBench.Action.GO_TO_LINE);
        //
        // await Task.Delay(5000, ct);
        //
        result = await _vsCodeApp.ExecuteCommand(new InsertSnippetVscCommand("abc def"));
        //
        // await Task.Delay(5000, ct);
        // _ = await ws.ExecuteCommandAsync(CommandTypes.WorkBench.Action.FOCUS_FIRST_EDITOR_GROUP);
        //
        // // await Task.Delay(5000, ct);
        // // _ = await ws.ExecuteCommandAsync(Commands.WorkBench.Action.CLOSE_OTHER_EDITORS);
        //
        // //   "command": "editor.action.insertSnippet",
        // //   "when": "editorTextFocus",
        // //   "args": {
        // //       "snippet": "\");\n$0"
        // //   }
        //
        // _outputHelper.WriteLine($"-----> {result}");
        // await Task.Delay(6_000, ct);
        // await ws.CloseAsync(ct);
    }

    private static void CopyDirectory(string sourceDirPath, string destDirPath)
    {
        if (!Directory.Exists(destDirPath))
        {
            Directory.CreateDirectory(destDirPath);
        }

        foreach (var filePath in Directory.GetFiles(sourceDirPath))
        {
            var fileName = Path.GetFileName(filePath);
            var destFilePath = Path.Combine(destDirPath, fileName);
            File.Copy(filePath, destFilePath, true);
        }

        foreach (var dirPath in Directory.GetDirectories(sourceDirPath))
        {
            var dirName = Path.GetFileName(dirPath);
            var destDirFullPath = Path.Combine(destDirPath, dirName);
            CopyDirectory(dirPath, destDirFullPath);
        }
    }

    private void InitRepoM(string path)
    {
        Directory.CreateDirectory(path);
        CopyDirectory("Configs", path);

        IAppDataPathProvider appDataPathProvider = A.Fake<IAppDataPathProvider>();
        A.CallTo(() => appDataPathProvider.AppDataPath).Returns(path);
        var fs = new FileSystem();
        var cacheStore = new DefaultRepositoryStore(appDataPathProvider, fs);

        var cache = new List<string>()
        {
            @"C:\Projects\RepoM-Git-repos\RepoM\",
            @"C:\Projects\RepoM-Git-repos\scriban\",
            @"C:\Projects\RepoM-Git-repos\test\Verify\",
            @"C:\Projects\RepoM-Git-repos\test\xunit\",
            @"C:\Projects\RepoM-Git-repos\test\TUnit\",
        };
        cacheStore.Set(cache);

        var fileAppSettingsService = new FileAppSettingsService(appDataPathProvider, fs, NullLogger.Instance);

        fileAppSettingsService.ReposRootDirectories =
        [
            "C:\\Projects\\RepoM-Git-repos\\",
        ];
    }

    [Fact]
    public async Task Scenario1()
    {
        var basePath = Path.GetTempPath();
        var path = Path.Combine(basePath, "RepoM-UI-testing", DateTime.UtcNow.ToString("yyyyMMdd-HHmmss"));
        InitRepoM(path);
        using IDisposable clearDirectory = ClearDirectoryOnDisposal(path);

        using var appRepoM = ApplicationFactory.LaunchRepoM(path);
        
        await Task.Delay(5_000);
        VisualStudioCodeWindow visualStudioCodeWindow = _vsCodeApp.WaitForWindow();
        _vsCodeApp.OpenFile(Path.Combine(path, "RepositoryActionsV2.yaml"));
        // using var appVsCode = ApplicationFactory.LaunchVsCode(System.IO.Path.Combine(path, "RepositoryActionsV2.yaml"));
        

        // using var appVsCode = _vsCode;

        // var result = await _vsCodeApp.ExecuteCommand(new OpenFileVscCommand(Path.Combine(path, "RepositoryActionsV2.yaml")));
        // result.Should().Be("ok");

        {
            await Task.Delay(10_000);

            appRepoM.WaitWhileMainHandleIsMissing(TimeSpan.FromSeconds(5));
            appRepoM.WaitWhileBusy(TimeSpan.FromSeconds(5));

            await RepoMWindow.ShowRepoM();
            await Task.Delay(100);

            await RepoMWindow.KeepRepoMOpenAsync();
            await Task.Delay(100);

            RepoMWindow repoM = await RepoMWindow.GetRepoMWindowAsync(appRepoM, _automation, _outputHelper);

            // undo keep open
            await RepoMWindow.KeepRepoMOpenAsync();

            _ = repoM.Hide();

            await Task.Delay(100);

            await visualStudioCodeWindow.FocusUsingMouseAsync();

            visualStudioCodeWindow.StatusBar.NotificationButton.Click();
            await Task.Delay(100);
            visualStudioCodeWindow.StatusBar.NotificationButton.Click();

            await visualStudioCodeWindow.FocusActiveEditorGroupAsync();

            await visualStudioCodeWindow.SelectAllAsync();
            await Task.Delay(100);
            await visualStudioCodeWindow.DeleteTextAsync();
            await Task.Delay(100);
            await visualStudioCodeWindow.SaveFileAsync();
            await Task.Delay(100);
            Keyboard.Type("action-menu:");
            Keyboard.Type(VirtualKeyShort.ENTER);
            await Task.Delay(100);
            Keyboard.Type("""
                          - type: browse-repository@1
                          - type: git-fetch@1
                          - type: git-pull@1
                          - type: git-push@1
                          - type: git-checkout@1
                          """);
            await Task.Delay(100);
            await visualStudioCodeWindow.SaveFileAsync();
            await Task.Delay(10_000);
            // await visualStudioCodeWindow.GoToLineAsync(6);

            Position pos = await visualStudioCodeWindow.GetCurrentCursorPositionAsync(p => p.Line == 6);
            _outputHelper.WriteLine($"L{pos.Line} C{pos.Column}");
            pos.Line.Should().Be(6);

            
            await visualStudioCodeWindow.SelectLineAsync();
            await Task.Delay(1000);
            Keyboard.Type(VirtualKeyShort.DELETE);
            await Task.Delay(1000);
            Keyboard.Type("end");
            await Task.Delay(1000);
            await visualStudioCodeWindow.GoToStartOfLineAsync();
            await Task.Delay(1000);
            // await vsCodeWindow.GoToEndOfLineAsync();
            // Keyboard.Type("Hello World");

            await repoM.ShowAsync();
            await repoM.SearchTextBox.FocusByMouseAsync();
            await repoM.SearchTextBox.TypeTextAsync("RepoM", Delays.DefaultKeyPressDelay);

            await Delays.DelayMediumAsync();


            repoM.RepositoryList.Items.Should().NotBeEmpty();    
            ListBoxItem firstItem = repoM.RepositoryList.Items[0];

            await firstItem.MoveMouseAndClick(MouseButton.Right, Delays.DefaultWaitUntilClick);
            await Delays.DelayMediumAsync();

            var ctxMenuItem = repoM.ContextMenu.Items[1].AsMenuItem();
            _outputHelper.WriteLine($"ctxMenuItem item text {ctxMenuItem.Text}");
            await ctxMenuItem.MoveMouseAndClick(MouseButton.Left, Delays.DefaultWaitUntilClick);

            var txt = firstItem.Text;
            _outputHelper.WriteLine($"Selected item text {txt}");

            repoM.Title.Should().Be("RepoM");
        }

        await Task.Delay(1000);
        appRepoM.Close();
        // appVsCode.Close();
    }

    private IDisposable ClearDirectoryOnDisposal(string path)
    {
        return new ClearDirectoryOnDisposal(path);
    }
}

internal sealed class ClearDirectoryOnDisposal : IDisposable
{
    private readonly string _path;

    public ClearDirectoryOnDisposal(string path)
    {
        _path = path;
    }

    public void Dispose()
    {
        Directory.Delete(_path, true);
    }
}