namespace RepoM.Plugin.EverythingFileSearch.Internal;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

/// <summary>Wrapper for Everything.</summary>
/// <remarks>See <see href="https://www.voidtools.com/support/everything/sdk/csharp/"/> for the SDK.</remarks>
internal static partial class Everything64Api
{
    private static readonly object _lock = new();
    private const int EVERYTHING_REQUEST_FILE_NAME = 0x00000001;
    private const int EVERYTHING_REQUEST_PATH = 0x00000002;

    public static IEnumerable<string> Search(string query)
    {
        lock (_lock)
        {
            try
            {
                const int BUFFER_SIZE = 1024;

                Everything_SetSearch(query);
                Everything_SetRequestFlags(EVERYTHING_REQUEST_FILE_NAME | EVERYTHING_REQUEST_PATH);
                Everything_SetMatchCase(false);

                if (!Everything_Query(true))
                {
                    return Enumerable.Empty<string>();
                }

                var nrResults = Everything_GetNumResults();

                if (nrResults == 0)
                {
                    return Enumerable.Empty<string>();
                }

                var buffer = new StringBuilder(BUFFER_SIZE);
                var result = new List<string>((int)nrResults);

                for (uint i = 0; i < nrResults; i++)
                {
                    buffer.Clear();
                    Everything_GetResultFullPathName(i, buffer, BUFFER_SIZE);
                    result.Add(buffer.ToString());
                }

                return result;
            }
            finally
            {
                Ignore(Everything_CleanUp);
            }
        }
    }

    public static bool IsInstalled()
    {
        lock (_lock)
        {
            try
            {
                _ = Everything_GetMajorVersion();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }

    private static void Ignore(Action action)
    {
        try
        {
            action.Invoke();
        }
        catch (Exception)
        {
            // intentionally do nothing
        }
    }

    [LibraryImport("Everything64.dll", StringMarshalling = StringMarshalling.Utf16)]
    private static partial void Everything_SetSearch(string lpSearchString);

    [LibraryImport("Everything64.dll")]
    private static partial void Everything_SetMatchCase([MarshalAs(UnmanagedType.Bool)] bool bEnable);

    [LibraryImport("Everything64.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool Everything_Query([MarshalAs(UnmanagedType.Bool)] bool bWait);

    [LibraryImport("Everything64.dll")]
    private static partial uint Everything_GetNumResults();

    [DllImport("Everything64.dll", CharSet = CharSet.Unicode)]
    private static extern void Everything_GetResultFullPathName(uint nIndex, StringBuilder lpString, uint nMaxCount);

    [LibraryImport("Everything64.dll")]
    private static partial void Everything_CleanUp();

    [LibraryImport("Everything64.dll")]
    private static partial uint Everything_GetMajorVersion();

    // Everything 1.4
    [LibraryImport("Everything64.dll")]
    private static partial void Everything_SetRequestFlags(uint dwRequestFlags);
}