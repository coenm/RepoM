namespace RepoM.Api.Git.ProcessExecution;

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using Microsoft.Extensions.Logging;
using RepoM.Core.Plugin.Repository;

public partial class ProcessExecutingGitCommander : IGitCommander
{
    private readonly ILogger _logger;
    private const string GIT_EXE = "git";
    
    /// <summary>
    /// Starting with version 1.7.10, Git uses UTF-8.
    /// Use this encoding for Git input and output.
    /// </summary>
    private static readonly Encoding _encoding = new UTF8Encoding(false, true);

    private static readonly Regex _validCommandName = ValidCommandNameRegex();

    public ProcessExecutingGitCommander(ILogger logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Runs the given git command, and returns the contents of its STDOUT.
    /// </summary>
    public string Command(IRepository repository, params string[] command)
    {
        var retVal = string.Empty;
        CommandOutputPipe(repository, output => retVal = output, command);
        return retVal;
    }

    /// <summary>
    /// Runs the given git command, and passes STDOUT through to the current process's STDOUT.
    /// </summary>
    public void CommandNoisy(IRepository repository, params string[] command)
    {
        CommandOutputPipe(repository, output => Trace.TraceInformation(output), command);
    }

    /// <summary>
    /// Runs the given git command, and redirects STDOUT to the provided action.
    /// </summary>
    public void CommandOutputPipe(IRepository repository, Action<string> handleOutput, params string[] command)
    {
        Time(command, () =>
            {
                AssertValidCommand(command);
                var output = Start(repository, command, RedirectStdout);
                handleOutput(output);
            });
    }

    /// <summary>
    /// The encoding used by a stream is a read-only property. Use this method to
    /// create a new stream based on <paramref name="stream"/> that uses
    /// the given <paramref name="encoding"/> instead.
    /// </summary>
    public static StreamWriter NewStreamWithEncoding(StreamWriter stream, Encoding encoding)
    {
        return new StreamWriter(stream.BaseStream, encoding);
    }

    private void Time(string[] command, Action action)
    {
        DateTime start = DateTime.Now;

        try
        {
            action();
        }
        finally
        {
            DateTime end = DateTime.Now;
            _logger.LogTrace("git command '{Command}' time took {Duration}", string.Join(" ", command), end - start);
        }
    }

    private static void RedirectStdout(ProcessStartInfo startInfo)
    {
        startInfo.RedirectStandardOutput = true;
        startInfo.StandardOutputEncoding = _encoding;
    }

    private static void RedirectStderr(ProcessStartInfo startInfo)
    {
        startInfo.RedirectStandardError = true;
        startInfo.StandardErrorEncoding = _encoding;
    }

    private static string Start(IRepository repository, string[] command, Action<ProcessStartInfo> initialize)
    {
        var timeout = (int)TimeSpan.FromSeconds(10).TotalMilliseconds;

        var psi = new ProcessStartInfo
            {
                FileName = GIT_EXE,
                WorkingDirectory = repository.Path,
            };

        SetArguments(psi, command);
        psi.CreateNoWindow = true;
        psi.UseShellExecute = false;
        psi.EnvironmentVariables["GIT_PAGER"] = "cat";
        RedirectStderr(psi);
        initialize(psi);

        var output = new StringBuilder();
        var error = new StringBuilder();

        using var outputWaitHandle = new AutoResetEvent(initialState: false);
        using var errorWaitHandle = new AutoResetEvent(initialState: false);
        using var process = new Process();
        process.StartInfo = psi;

        process.OutputDataReceived += (sender, e) =>
            {
                if (e.Data == null)
                {
                    try
                    {
                        outputWaitHandle.Set();
                    }
                    catch (ObjectDisposedException)
                    {
                        // if the wait handle was disposed,
                        // we can ignore the call to .Set()
                    }
                }
                else
                {
                    output.AppendLine(e.Data);
                }
            };

        process.ErrorDataReceived += (sender, e) =>
            {
                if (e.Data == null)
                {
                    try
                    {
                        errorWaitHandle.Set();
                    }
                    catch (ObjectDisposedException)
                    {
                        // if the wait handle was disposed,
                        // we can ignore the call to .Set()
                    }
                }
                else
                {
                    error.AppendLine(e.Data);
                }
            };

        try
        {
            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            if (process.WaitForExit(timeout) &&
                outputWaitHandle.WaitOne(timeout) &&
                errorWaitHandle.WaitOne(timeout))
            {
                // Process completed. Check process.ExitCode here.
                return output.ToString();
            }

            // Timed out.
            return error?.ToString() ?? "Unknown error";
        }
        finally
        {
            if (!process.WaitForExit((int)TimeSpan.FromSeconds(10).TotalMilliseconds))
            {
                throw new GitCommandException("Command did not terminate.");
            }

            if (process.ExitCode != 0)
            {
                throw new GitCommandException($"Command exited with error code: {process.ExitCode}\n{error?.ToString() ?? "Unknown error"}");
            }
        }
    }

    private static void SetArguments(ProcessStartInfo startInfo, params string[] args)
    {
        startInfo.Arguments = string.Join(" ", args.Select(QuoteProcessArgument).ToArray());
    }

    private static string QuoteProcessArgument(string arg)
    {
        return arg.Contains(' ') ? "\"" + arg + "\"" : arg;
    }
    
    private static void AssertValidCommand(string[] command)
    {
        if (command.Length < 1 || !_validCommandName.IsMatch(command[0]))
        {
            throw new Exception("bad git command: " + (command.Length == 0 ? "" : command[0]));
        }
    }

    [GeneratedRegex("^[a-z0-9A-Z_-]+$", RegexOptions.Compiled)]
    private static partial Regex ValidCommandNameRegex();
}