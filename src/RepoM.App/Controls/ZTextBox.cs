namespace RepoM.App.Controls;

using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Input;

/// <summary>
/// The ZTextBox class is a custom control that extends the TextBox class in WPF. It adds additional functionality to handle specific key events. Here's a brief explanation:
/// •	Namespace: RepoM.App.Controls
/// •	Base Class: TextBox
/// •	Event: Finish - This event is triggered when certain keys (Down, Return, Enter) are pressed.
/// •	Key Handling:
/// •	Clears the text when the Escape key is pressed.
/// •	Triggers the Finish event when any of the keys in the FinisherKeys list are pressed.
/// This custom control can be useful in scenarios where you need to perform specific actions based on key presses within a text box.
/// </summary>
public class ZTextBox : TextBox
{
    public event EventHandler? Finish;

    protected override void OnKeyUp(KeyEventArgs e)
    {
        base.OnKeyUp(e);

        if (e.Key == Key.Escape)
        {
            this.Clear();
        }

        if (FinisherKeys.Contains(e.Key))
        {
            this.Finish?.Invoke(this, EventArgs.Empty);
        }
    }

    private static List<Key> FinisherKeys { get; } =
        [
            Key.Down,
            Key.Return,
            Key.Enter,
        ];
}
