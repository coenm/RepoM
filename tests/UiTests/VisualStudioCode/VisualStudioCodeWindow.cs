namespace UiTests.VisualStudioCode;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Input;
using FlaUI.Core.WindowsAPI;
using UiTests.Utils;
using UiTests.VisualStudioCode.WebSockets;
using Xunit.Abstractions;
using AutomationElement = FlaUI.Core.AutomationElements.AutomationElement;

public class VisualStudioCodeWindow : Window, IDisposable
{
    private readonly VisualStudioWebSocketAutomation _vscWebSocketAutomation;
    private readonly ITestOutputHelper _outputHelper;
    private List<IDisposable> _disposables = new();

    public VisualStudioCodeWindow(
        FrameworkAutomationElementBase frameworkAutomationElement,
        VisualStudioWebSocketAutomation vscWebSocketAutomation,
        ITestOutputHelper outputHelper)
        : base(frameworkAutomationElement)
    {
        _vscWebSocketAutomation = vscWebSocketAutomation ?? throw new ArgumentNullException(nameof(vscWebSocketAutomation));
        _outputHelper = outputHelper ?? throw new ArgumentNullException(nameof(outputHelper));

        _disposables.Add(_vscWebSocketAutomation.FocusUpdated.Subscribe(focussed => HasFocus = focussed));
    }

    public bool HasFocus { get; private set; }

    public StatusBar StatusBar
    {
        get
        {
            AutomationElement result = FindFirstDescendant("workbench.parts.statusbar");
            return new StatusBar(result, _outputHelper);
        }
    }

    public int Indentation { get; private set; }

    public async Task<Position> GetCurrentCursorPositionAsync(Predicate<Position>? untilCheck = null)
    {
        PositionElement positionElement = StatusBar.PositionElement;
        Position result = positionElement.Position;

        if (untilCheck == null)
        {
            return result;
        }

        var counter = 0;
        while (!untilCheck(result) && counter < 100)
        {
            _outputHelper.WriteLine($"RETRY ({counter}): {result.Line} {result.Column}");
            await Task.Delay(5);
            counter++;
            try
            {
                result = positionElement.Position;
            }
            catch (Exception e)
            {
                _outputHelper.WriteLine(e.Message);
            }
        }

        return result;
    }

    public async Task FocusActiveEditorGroupAsync()
    {
        await VsCodeOpenCommandPalletAsync();
        await Task.Delay(5);
        Keyboard.Type("Focus active editor Group");
        await Task.Delay(5);
        Keyboard.Type(VirtualKeyShort.ENTER);
        await Task.Delay(100);
    }

    private Task VsCodeOpenCommandPalletAsync()
    {
        Keyboard.TypeSimultaneously(
            VirtualKeyShort.CONTROL,
            VirtualKeyShort.SHIFT,
            VirtualKeyShort.KEY_P);
        return Task.CompletedTask;
    }

    public async Task FocusUsingMouseAsync()
    {
        this.WaitUntilClickable(TimeSpan.FromSeconds(5));

        if (!TryGetClickablePoint(out var p))
        {
            throw new InvalidOperationException("Could not get clickable point");
        }

        Mouse.MovePixelsPerMillisecond *= 3;
        Mouse.MoveTo(p);
        await Task.Delay(1000);
        Mouse.Click();
        await Task.Delay(1000);
    }

    public Task GoToLineAsync(int lineNumber)
    {
        Position pos = StatusBar.PositionElement.Position;
        if (pos.Line == lineNumber)
        {
            return Task.CompletedTask;
        }

        if (Math.Abs(pos.Line - lineNumber) < 5)
        {
            return GoToLineUsingArrow(lineNumber);
        }

        return GoToLineUsingCtrlGAsync(lineNumber);
    }

    private async Task GoToLineUsingCtrlGAsync(int lineNumber)
    {
        Keyboard.TypeSimultaneously(
            VirtualKeyShort.CONTROL,
            VirtualKeyShort.KEY_G);
        await Task.Delay(100);
        Keyboard.Type(lineNumber.ToString());
        await Task.Delay(100);
        Keyboard.Type(VirtualKeyShort.ENTER);
        await Task.Delay(100);
    }

    private async Task GoToLineUsingArrow(int lineNumber)
    {
        var currentPos = await GetCurrentCursorPositionAsync();

        while (currentPos.Line != lineNumber)
        {
            var delta = Math.Abs(currentPos.Line - lineNumber);

            if (currentPos.Line < lineNumber)
            {
                for (var i = 0; i < delta; i++)
                {
                    Keyboard.Type(VirtualKeyShort.DOWN);
                    await Task.Delay(20);
                }
            }
            else
            {
                for (var i = 0; i < delta; i++)
                {
                    Keyboard.Type(VirtualKeyShort.UP);
                    await Task.Delay(20);
                }
            }

            currentPos = await GetCurrentCursorPositionAsync(c => c.Line == lineNumber);
        }
    }

    public Task GoToStartOfLineAsync()
    {
        Keyboard.Type(VirtualKeyShort.HOME);
        return Task.Delay(100);
    }

    public Task GoToEndOfLineAsync()
    {
        Keyboard.Type(VirtualKeyShort.END);
        return Task.Delay(100);
    }

    public async Task SelectLineAsync()
    {
        Keyboard.Type(VirtualKeyShort.HOME);
        await Task.Delay(1000);
        await Delays.DelaySmallAsync();
        Keyboard.TypeSimultaneously(VirtualKeyShort.SHIFT, VirtualKeyShort.END);
        await Delays.DelayMediumAsync();
    }

    public void Dispose()
    {
        _vscWebSocketAutomation.Dispose();
        foreach (IDisposable d in _disposables)
        {
            d.Dispose();
        }
    }
}