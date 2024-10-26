namespace UiTests;

using System;
using System.Threading.Tasks;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Input;
using FlaUI.Core.WindowsAPI;
using FlaUI.UIA3;
using FluentAssertions;
using UiTests.Extensions;
using UiTests.Framework;
using UiTests.RepoM;
using UiTests.Utils;
using UiTests.VisualStudioCode;
using Xunit;
using Xunit.Abstractions;

[SetTestName]
public class NotePadTest
{
    private readonly ITestOutputHelper _outputHelper;

    public NotePadTest(ITestOutputHelper outputHelper)
    {
        _outputHelper = outputHelper ?? throw new ArgumentNullException(nameof(outputHelper));
    }

    // make sure no vs code instance is running.

    [Fact]
    public async Task NotepadLaunchTest()
    {
        using var appVsCode = ApplicationFactory.LaunchVsCode(@"C:\Users\Munckhof CJJ\AppData\Roaming\RepoM\RepositoryActionsV2.yaml");
        using var appRepoM = ApplicationFactory.LaunchRepoM();

        using (var automationRepoM = new UIA3Automation())
        // using (var automationVsCode = new UIA3Automation())
        {
            var automationVsCode = automationRepoM;

            await Task.Delay(20_000);
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
}