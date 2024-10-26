namespace UiTests.Extensions;

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Input;
using UiTests;
using UiTests.Utils;

public static class AutomationElementExtensionsExtensions
{
    public static T As<T>(this AutomationElement self, params object[] args) where T : AutomationElement
    {
        if (self == null)
        {
            return default!;
        }

        var args2 = new List<object>
            {
                self.FrameworkAutomationElement,
            };
        args2.AddRange(args);

        return (T)Activator.CreateInstance(typeof(T), args2.ToArray())!;
    }

    public static async Task MoveMouseAsync(this AutomationElement element)
    {
        Point point = element.GetClickablePoint();
        await Delays.DelaySmallAsync();
        Mouse.MoveTo(point);
        await Delays.DelaySmallAsync();
    }

    public static async Task MoveMouseAndClick(this AutomationElement element, MouseButton btn = MouseButton.Left, TimeSpan? delayBeforeClick = null)
    {
        await element.MoveMouseAsync();

        if (delayBeforeClick.HasValue)
        {
            await Task.Delay(delayBeforeClick.Value);
        }

        Mouse.Click(btn);
        await Delays.DelaySmallAsync();
    }

    public static Task FocusByMouseAsync(this AutomationElement element, TimeSpan? delayBeforeClick = null)
    {
        return MoveMouseAndClick(element, MouseButton.Left, delayBeforeClick);
    }
    
    public static Task TypeTextAsync(this TextBox textBox, string text)
    {
        return textBox.TypeTextAsync(text, TimeSpan.Zero);
    }

    public static async Task TypeTextAsync(this TextBox textBox, string text, TimeSpan delayBetweenChars)
    {
        if (!textBox.IsEnabled)
        {
            throw new Exception("TextBox is not enabled");
        }

        if (text == string.Empty)
        {
            await Delays.DelaySmallAsync();
            return;
        }

        if (delayBetweenChars > TimeSpan.Zero)
        {
            foreach (var character in text)
            {
                Keyboard.Type(character);
                await Task.Delay(delayBetweenChars);
            }
        }
        else
        {
            Keyboard.Type(text);
        }

        await Delays.DelaySmallAsync();
    }
}