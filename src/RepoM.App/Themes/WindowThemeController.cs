namespace RepoM.App.Themes;

using System;
using System.IO;
using System.Security;
using System.Windows;
using Microsoft.Win32;
using RepoM.Api.Common;

internal sealed class WindowThemeController : IDisposable
{
    private readonly Window _window;
    private readonly IPreviewSystemTheme _systemTheme;
    private readonly Action<Exception?> _reportError;
    private readonly Action _beforeChange;
    private readonly ResourceDictionary _resources;
    private AppTheme _selectedTheme;
    private AppTheme _effectiveTheme = AppTheme.Dark;
    private bool _disposed;

    public WindowThemeController(Window window, ResourceDictionary resources, IPreviewSystemTheme systemTheme,
        Action beforeChange, Action<Exception?> reportError)
    {
        _window = window;
        _resources = resources;
        _systemTheme = systemTheme;
        _beforeChange = beforeChange;
        _reportError = reportError;
        _systemTheme.Changed += SystemTheme_Changed;
        _window.Closed += Window_Closed;
    }

    public void SetTheme(AppTheme theme)
    {
        if (!Enum.IsDefined(theme))
        {
            throw new ArgumentOutOfRangeException(nameof(theme), theme, "Unknown application theme.");
        }

        _selectedTheme = theme;
        Apply();
    }

    private void Apply()
    {
        bool isLight;
        try
        {
            isLight = _selectedTheme == AppTheme.System ? _systemTheme.IsLight : _selectedTheme == AppTheme.Light;
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or SecurityException)
        {
            _reportError(exception);
            return;
        }

        _reportError(null);
        AppTheme effective = isLight ? AppTheme.Light : AppTheme.Dark;
        if (effective == _effectiveTheme)
        {
            return;
        }

        _beforeChange();
        _resources.Source = new Uri($"/RepoM;component/Themes/WorktreePreview.{effective}.xaml", UriKind.Relative);
        _effectiveTheme = effective;
    }

    private void SystemTheme_Changed(object sender, UserPreferenceChangedEventArgs e)
    {
        _ = _window.Dispatcher.InvokeAsync(() =>
        {
            if (!_disposed && _selectedTheme == AppTheme.System)
            {
                Apply();
            }
        });
    }

    private void Window_Closed(object? sender, EventArgs e) => Dispose();

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        _systemTheme.Changed -= SystemTheme_Changed;
        _window.Closed -= Window_Closed;
    }
}
