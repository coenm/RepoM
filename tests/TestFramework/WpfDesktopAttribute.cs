namespace RepoM.Tests.TestFramework;

using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using Xunit.Sdk;

internal sealed class WpfDesktopAttribute : BeforeAfterTestAttribute
{
    private const int SharingViolation = unchecked((int)0x80070020);
    private static readonly string _lockPath = Path.Combine(Path.GetTempPath(), "RepoM.WpfDesktopTests.lock");
    private FileStream? _lease;

    public override void Before(MethodInfo methodUnderTest) => _lease = Acquire();

    public override void After(MethodInfo methodUnderTest) => _lease?.Dispose();

    internal static FileStream Acquire()
    {
        // Separate test hosts still share the same foreground window and mouse capture.
        TimeSpan timeout = TimeSpan.FromSeconds(45);
        var wait = Stopwatch.StartNew();
        while (true)
        {
            try
            {
                return new FileStream(_lockPath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
            }
            catch (IOException exception) when (exception.HResult == SharingViolation)
            {
                if (wait.Elapsed >= timeout)
                {
                    throw new TimeoutException("Timed out waiting for exclusive access to the WPF test desktop.", exception);
                }

                Thread.Sleep(50);
            }
        }
    }
}
