namespace UiTests;

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Threading.Tasks;
using FakeItEasy;
using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Input;
using FlaUI.Core.WindowsAPI;
using FlaUI.UIA3;
using FluentAssertions;
using global::RepoM.Api.Common;
using global::RepoM.Api.Git;
using global::RepoM.Api.IO;
using global::RepoM.Core.Plugin.Common;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Newtonsoft.Json;
using UiTests.Extensions;
using UiTests.Framework;
using UiTests.RepoM;
using UiTests.Utils;
using UiTests.VisualStudioCode;
using Xunit;
using Xunit.Abstractions;

//C:\Projects\RepoM-Git-repos


[SetTestName]
public class NotePadTest
{
    private readonly ITestOutputHelper _outputHelper;

    public NotePadTest(ITestOutputHelper outputHelper)
    {
        _outputHelper = outputHelper ?? throw new ArgumentNullException(nameof(outputHelper));
    }

    // make sure no vs code instance is running.


    public static void CopyDirectory(string sourceDirPath, string destDirPath)
    {
        if (!Directory.Exists(destDirPath))
        {
            Directory.CreateDirectory(destDirPath);
        }

        foreach (string filePath in Directory.GetFiles(sourceDirPath))
        {
            string fileName = Path.GetFileName(filePath);
            string destFilePath = Path.Combine(destDirPath, fileName);
            File.Copy(filePath, destFilePath, true);
        }

        foreach (string dirPath in Directory.GetDirectories(sourceDirPath))
        {
            string dirName = Path.GetFileName(dirPath);
            string destDirFullPath = Path.Combine(destDirPath, dirName);
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
    public async Task NotepadLaunchTest()
    {
        var basePath = Path.GetTempPath();
        basePath = Path.Combine("C:", "tmp");
        var path = Path.Combine(basePath, "RepoM-UI-testing", DateTime.UtcNow.ToString("yyyyMMdd-HHmmss"));
        InitRepoM(path);
        using var clearDirectory = ClearDirectoryOnDisposal(path);
        using var appVsCode = ApplicationFactory.LaunchVsCode(@"C:\Users\Munckhof CJJ\AppData\Roaming\RepoM\RepositoryActionsV2.yaml");
        using var appRepoM = ApplicationFactory.LaunchRepoM(path);

        using (var automationRepoM = new UIA3Automation())
        // using (var automationVsCode = new UIA3Automation())
        {
            var automationVsCode = automationRepoM;

            await Task.Delay(10_000);
            appRepoM.WaitWhileMainHandleIsMissing(TimeSpan.FromSeconds(5));
            appRepoM.WaitWhileBusy(TimeSpan.FromSeconds(5));
            appVsCode.WaitWhileMainHandleIsMissing(TimeSpan.FromSeconds(20));
            appVsCode.WaitWhileBusy(TimeSpan.FromSeconds(20));

            VisualStudioCodeWindow visualStudioCodeWindow = appVsCode.GetMainWindow(automationVsCode).As<VisualStudioCodeWindow>(_outputHelper);
            visualStudioCodeWindow.Should().NotBeNull();

            await RepoMWindow.ShowRepoM();
            await Task.Delay(100);

            await RepoMWindow.KeepRepoMOpenAsync();
            await Task.Delay(100);

            RepoMWindow repoM = await RepoMWindow.GetRepoMWindowAsync(appRepoM, automationRepoM, _outputHelper);

            // undo keep open
            await RepoMWindow.KeepRepoMOpenAsync();

            _ = repoM.Hide();

            await Task.Delay(100);

            await visualStudioCodeWindow.FocusUsingMouseAsync();

            visualStudioCodeWindow.StatusBar.NotificationButton.Click();
            await Task.Delay(100);
            visualStudioCodeWindow.StatusBar.NotificationButton.Click();

            await visualStudioCodeWindow.FocusActiveEditorGroupAsync();
            await visualStudioCodeWindow.GoToLineAsync(6);

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
        appVsCode.Close();
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