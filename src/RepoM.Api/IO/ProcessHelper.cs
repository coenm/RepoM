namespace RepoM.Api.IO;

using System;
using System.Diagnostics;
using Microsoft.Extensions.Logging;

public static class ProcessHelper
{
    public static void StartProcess(string process, string arguments, ILogger logger)
    {
        try
        {
            Process.Start(process, arguments);
            return;
        }
        catch (Exception)
        {
            // swallow, retry below.
        }

        try
        {
            var psi = new ProcessStartInfo(process, arguments)
                {
                    UseShellExecute = true,
                };
            Process.Start(psi);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to start process {Process} with arguments {Arguments}", process, arguments);
        }
    }
}