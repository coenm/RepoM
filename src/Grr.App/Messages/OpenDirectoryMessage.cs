namespace Grr.App.Messages;

using System.Diagnostics;
using System.IO.Abstractions;
using System.Runtime.InteropServices;
using RepoM.Ipc;

[System.Diagnostics.DebuggerDisplay("{GetRemoteCommand()}")]
public class OpenDirectoryMessage : DirectoryMessage
{
    public OpenDirectoryMessage(RepositoryFilterOptions filter, IFileSystem fileSystem)
        : base(filter, fileSystem)
    {
    }

    protected override void ExecuteExistingDirectory(string directory)
    {
        var directoryInQuotes = $"\"{directory}\"";

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            Process.Start(new ProcessStartInfo(directoryInQuotes) { UseShellExecute = true, });
        }
        else
        {
            Process.Start(new ProcessStartInfo("open", directoryInQuotes));
        }
    }

    public override bool ShouldWriteRepositories(Repository[] repositories)
    {
        return true;
    }
}