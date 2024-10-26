namespace UiTests.RepoM;

using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Conditions;
using FlaUI.Core.Definitions;
using FlaUI.Core.Input;
using FlaUI.Core.Tools;
using FlaUI.Core.WindowsAPI;
using FlaUI.UIA3;
using FluentAssertions;
using UiTests.Extensions;
using UiTests.Utils;
using Xunit.Abstractions;

public class RepoMWindow : Window
{
    private readonly ITestOutputHelper _outputHelper;

    public RepoMWindow(FrameworkAutomationElementBase frameworkAutomationElement, ITestOutputHelper outputHelper)
        : base(frameworkAutomationElement)
    {
        _outputHelper = outputHelper;
    }

    public bool IsVisible
    {
        get
        {
            try
            {
                var isAvailable = this.IsAvailable;
                var isEnabled = this.IsEnabled;
                var isOffscreen = this.IsOffscreen;
                _outputHelper.WriteLine("Visible:");
                _outputHelper.WriteLine($" IsAvailable:{isAvailable}");
                _outputHelper.WriteLine($" IsEnabled:{isEnabled}");
                _outputHelper.WriteLine($" IsOffscreen:{isOffscreen}");
                return !isOffscreen;
            }
            catch (Exception e)
            {
                _outputHelper.WriteLine("Could not determine if RepoM is visible. Returning false. {0}", e.Message);
                return false;
            }
        }
    }

    public TextBox SearchTextBox
    {
        get
        {
            TextBox? result = FindFirstDescendant("txtFilter").AsTextBox();
            result.Should().NotBeNull();
            return result;
        }
    }

    public ListBox RepositoryList
    {
        get
        {
            ListBox? result = FindFirstDescendant("lstRepositories").AsListBox();
            result.Should().NotBeNull();
            return result;
        }
    }

    public static Task KeepRepoMOpenAsync()
    {
        Keyboard.TypeSimultaneously(VirtualKeyShort.F12);
        return Task.CompletedTask;
    }

    public async Task ShowAsync()
    {
        if (!IsVisible)
        {
            _outputHelper.WriteLine("RepoM is not visible -> Show");
            // ((Window)this).SetForeground();
            await ShowRepoM();
        }
        else
        {
            _outputHelper.WriteLine("RepoM is already visible");
        }

        await Delays.DelaySmallAsync();
    }

    public bool Hide()
    {
        if (IsVisible)
        {
            Focus();
        }

        var counter = 0;
        while (IsVisible && counter < 5)
        {
            counter++;
            Keyboard.Type(VirtualKeyShort.ESC);
        }

        return !IsVisible;
    }
    
    public static async Task ShowRepoM()
    {
        Keyboard.TypeSimultaneously(
            VirtualKeyShort.CONTROL,
            VirtualKeyShort.ALT,
            VirtualKeyShort.KEY_R);
        await Task.Yield();
    }

    public static async Task<RepoMWindow> GetRepoMWindowAsync(Application app, UIA3Automation automation, ITestOutputHelper outputHelper)
    {
        // using var app = FlaUI.Core.Application.Attach(processName);
        var visibleWindows = automation.GetDesktop();

        app.WaitWhileMainHandleIsMissing(TimeSpan.FromSeconds(5));

        // doesn't work:
        // var window = visibleWindows.FindFirstChild(cf => cf.ByControlType(ControlType.Window));

        int count = 0;

        AutomationElement[] windows = [];

        while (windows.Length == 0 && count < 10)
        {
            count++;
            windows = visibleWindows.FindAllChildren(e => e.ByProcessId(app.ProcessId)).ToArray();
            if (windows.Length == 0)
            {
                await Task.Delay(500);
            }
        }

        windows.Should().HaveCount(1);
        return windows[0].As<RepoMWindow>(outputHelper);
    }

    // protected Button ZoomInButton => FindFirstNested(cf => new ConditionBase[] {
    //         cf.ByControlType(ControlType.StatusBar),
    //         cf.ByControlType(ControlType.Pane),
    //         cf.ByText("+")
    //     }).AsButton();
    //
    //
    // protected TextBox ZoomText => FindFirstNested(cf => new ConditionBase[] {
    //         cf.ByControlType(ControlType.StatusBar),
    //         cf.ByControlType(ControlType.Pane),
    //         cf.ByControlType(ControlType.Text)
    //     }).AsTextBox();

  
    // public void ZoomIn()
    // {
    //     var currentZoom = ZoomText.Text;
    //     ZoomInButton.Invoke();
    //     WaitUntilZoomTextChanged(currentZoom);
    // }
    //
    // public int GetCurrentZoomPercent()
    // {
    //     var zoomText = ZoomText.Text;
    //     var zoomNumberString = Regex.Match(zoomText, @"[0-9]+").ToString();
    //     return Convert.ToInt32(zoomNumberString);
    // }
    //
    // private void WaitUntilZoomTextChanged(string oldZoomText)
    // {
    //     Retry.WhileTrue(() => oldZoomText == ZoomText.Text, timeout: TimeSpan.FromSeconds(1), throwOnTimeout: true, timeoutMessage: "Failed to change zoom");
    // }
}