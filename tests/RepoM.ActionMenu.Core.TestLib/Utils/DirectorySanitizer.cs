namespace RepoM.ActionMenu.Core.TestLib.Utils;

using System;
using System.IO;
using System.Runtime.InteropServices;

internal static class DirectorySanitizer
{
    public const char DIRECTORY_SEPARATOR_CHAR = '/'; // Path.DirectorySeparatorChar

    private static readonly Lazy<bool> _executingOnWindows = new(() => RuntimeInformation.IsOSPlatform(OSPlatform.Windows));

    public static string Sanitize(in string input)
    {
        return input.Replace('\\', DIRECTORY_SEPARATOR_CHAR);
    }

    public static string PathCombine(params string[] paths)
    {
        return Sanitize(Path.Combine(paths));
    }

    public static string ToOperatingSystemPath(in string path)
    {
        return _executingOnWindows.Value
            ? path.Replace(DIRECTORY_SEPARATOR_CHAR, '\\')
            : path;
    }
}