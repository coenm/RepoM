namespace RepoM.Api.IO;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Extensions.Logging;
using static RepoM.Api.IO.ProcessHelper;

public static class ProcessHelper
{
    public static void StartProcess(string process, string arguments, ILogger logger)
    {
        List<Exception> exceptionList = [];

        var psi = new ProcessStartInfo()
        {
            FileName = process,       // The file to run.
            Arguments = arguments,   // Arguments to pass to the process.
            UseShellExecute = false,  // Execute the process directly rather than using the shell. Similar to double-clicking the file in Explorer or the RunAs command.
        };

        try
        {
            var processState = Process.Start(psi);

            // This is probably never null as RunAs always throws an exception if there is a problem, but we check just in case.
            if (null == processState) { throw new Exception("Process failed to start."); }

            logger.LogInformation("Successfully started process {Process} with arguments {Arguments} via primary method.", process, arguments);
            return;
        }
        catch (Exception ex)
        {
            /*
             * This method wil fail if the process is:
             * a. not found in the PATH system variable
             * b. not found in %LOCALAPPDATA%\Microsoft\WindowsApps
             * c. %LOCALAPPDATA%\Microsoft\WindowsApps is not in the PATH.
             */
            logger.LogInformation(ex, "Failed to start process {Process} with arguments {Arguments} via primary method. Attempt via secondary method.", process, arguments);
            exceptionList.Add(ex);
        }

        try
        {
            psi.UseShellExecute = true; // Uses shell execute to allow for file associations to work. The default is true on .NET Framework apps and false on .NET Core apps.
            psi.WindowStyle = ProcessWindowStyle.Hidden; // Hides the shell/terminal window.
            var processState = Process.Start(psi);

            // Sometimes the shell execute method fails to start the process but does not throw an exception so we need to check if the process is null.
            if (null == processState) { throw new Exception("Process failed to start."); }

            logger.LogInformation("Successfully started process {Process} with arguments {Arguments} via secondary method.", process, arguments);
            return;
        }
        catch (Exception ex)
        {
            logger.LogInformation(ex, "Failed to start process {Process} with arguments {Arguments} via secondary method.", process, arguments);
            exceptionList.Add(ex);
        }

        /*
         * We only log the exceptions as errors if we failed to start the process through all the methods.
         * Otherwise, we log them as information.
         */
        foreach ((Exception currentException, var index) in exceptionList.WithIndex())
        {
            logger.LogError(currentException, "Failed to start process {Process} with arguments {Arguments}. [{Index}/{Count}]", process, arguments, index, exceptionList.Count);
        }

    }

}


// [CODE REFACTORING] Move this class somewhere else

// ReSharper disable once InconsistentNaming
public static class IEnumerableExtensions
{
    public static IEnumerable<(T item, int index)> WithIndex<T>(this IEnumerable<T> self)
        => self?.Select((item, index) => (item, index)) ?? new List<(T, int)>();
}

