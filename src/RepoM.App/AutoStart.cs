namespace RepoM.App;

using System;
using System.Reflection;
using Microsoft.Win32;

public static class AutoStart
{
    private const string REG_KEY = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";

    public static void SetStartup(string appName, bool startup)
    {
        RegistryKey? key = Registry.CurrentUser.OpenSubKey(REG_KEY, true);

        if (startup)
        {
            key?.SetValue(appName, GetAppPath());
        }
        else
        {
            key?.DeleteValue(appName, false);
        }
    }

    public static bool IsStartup(string appName)
    {
        RegistryKey? key = Registry.CurrentUser.OpenSubKey(REG_KEY);
        return IsStartup(key, appName);
    }

    private static bool IsStartup(RegistryKey? key, string appName)
    {
        return GetValueAsString(key, appName)
            .Equals(GetAppPath(), StringComparison.OrdinalIgnoreCase);
    }

    private static string GetValueAsString(RegistryKey? key, string appName)
    {
        var value = key?.GetValue(appName);
        return value?.ToString() ?? string.Empty;
    }

    private static string GetAppPath()
    {
        var path = Assembly.GetEntryAssembly()!.Location;

        // Dirty hack. We need the exe instead of dll (this has probably something to do with the upgrade to net6)
        if (path.EndsWith(".dll", StringComparison.CurrentCultureIgnoreCase))
        {
            path = path[..^4] + ".exe";
        }

        return $"\"{path}\"";
    }
}