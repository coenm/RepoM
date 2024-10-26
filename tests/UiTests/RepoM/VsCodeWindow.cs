namespace UiTests.RepoM;

using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;
using FlaUI.Core.Input;
using FlaUI.Core.WindowsAPI;
using FluentAssertions;
using UiTests.Utils;
using Xunit.Abstractions;
using AutomationElement = FlaUI.Core.AutomationElements.AutomationElement;
using NotSupportedException = FlaUI.Core.Exceptions.NotSupportedException;

public record Position(int Line, int Column);

public class VsCodeWindow : Window
{
    private readonly ITestOutputHelper _outputHelper;

    public VsCodeWindow(FrameworkAutomationElementBase frameworkAutomationElement, ITestOutputHelper outputHelper)
        : base(frameworkAutomationElement)
    {
        _outputHelper = outputHelper ?? throw new ArgumentNullException(nameof(outputHelper));
    }

    public Position CurrentCursorPosition
    {
        get
        {
            var selection = StatusBar.FindFirstDescendant("status.editor.selection");
            selection.Should().NotBeNull();

            var children = selection.FindAllChildren();
            children.Should().HaveCount(1);
            
            var text = children[0].Name.Trim();

            if (string.IsNullOrWhiteSpace(text))
            {
                throw new NotSupportedException("Could not find line and column from empty text");
            }

            // todo, also other type Ln 3, Col 4 (3, 4) stuf.
            var regex = new Regex("Ln\\s([0-9]+),\\sCol\\s([0-9]+)", RegexOptions.Compiled | RegexOptions.Singleline);
            Match result = regex.Match(text);
            if (result.Success)
            {
                return new Position(
                    int.Parse(result.Groups[1].Value),
                    int.Parse(result.Groups[2].Value));
            }

            throw new NotSupportedException("Could not find line and column from " + text);
        }
    }

    public Button NotificationButton
    {
        get
        {
            AutomationElement selection = StatusBar.FindFirstDescendant("status.notifications");
            selection.Should().NotBeNull();

            Button btn = selection.FindFirstChild(cf => cf.ByControlType(ControlType.Button)).AsButton();
            btn.Should().NotBeNull();

            return btn;
        }
    }

    public AutomationElement StatusBar
    {
        get
        {
            AutomationElement? result = FindFirstDescendant("workbench.parts.statusbar"); // StatusBar
            result.Should().NotBeNull();
            return result;
        }
    }

    public int Indentation
    {
        get
        {
            var item = StatusBar.FindFirstDescendant("status.editor.indentiation");
            // only one child should be present
            item.FindAllChildren().Should().HaveCount(1);
            var text = item.FindAllChildren()[0].Name.Trim();
    
            _outputHelper.WriteLine("Indentation: " + text);
    
            return text.Length; //todo
        }
    }
    
    public async Task<Position> GetCurrentCursorPositionAsync(Predicate<Position>? untilCheck = null)
    {
        Position result = CurrentCursorPosition;

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
                result = CurrentCursorPosition;
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

    private  Task VsCodeOpenCommandPalletAsync()
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

        if (!this.TryGetClickablePoint(out var p))
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
        var pos = CurrentCursorPosition;
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
                for (int i = 0; i < delta; i++)
                {
                    Keyboard.Type(VirtualKeyShort.DOWN);
                    await Task.Delay(20);
                }
            }
            else
            {
                for (int i = 0; i < delta; i++)
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
}