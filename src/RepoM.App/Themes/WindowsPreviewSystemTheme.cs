namespace RepoM.App.Themes;

using System.IO;
using Microsoft.Win32;

public enum WorktreePreviewTheme
{
    Dark,
    Light,
    System,
}

public interface IPreviewSystemTheme
{
    bool IsLight { get; }

    event UserPreferenceChangedEventHandler? Changed;
}

public sealed class WindowsPreviewSystemTheme : IPreviewSystemTheme
{
    public bool IsLight => Registry.GetValue(
        @"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Themes\Personalize",
        "AppsUseLightTheme",
        null) switch
    {
        0 => false,
        1 => true,
        null => true, // Windows uses light app mode when no preference is stored.
        _ => throw new InvalidDataException("Windows AppsUseLightTheme must be 0 or 1."),
    };

    public event UserPreferenceChangedEventHandler? Changed
    {
        add => SystemEvents.UserPreferenceChanged += value;
        remove => SystemEvents.UserPreferenceChanged -= value;
    }
}
