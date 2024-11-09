namespace UiTests.VisualStudioCode.WebSockets.Commands;

using System;
using System.Collections.Generic;
using Lucene.Net.Analysis.Hunspell;

/// <summary>
/// Opens the provided resource in the editor.
/// </summary>
public sealed class OpenFileVscCommand : VscCommand
{
    public OpenFileVscCommand(string filename)
        // : base("workbench.action.files.openFolder")
        // : base("workbench.action.files.openFile")
        // : base("vscode.open")
        : base("open-file-command.openFile")
    {
        // Args = new[]
        // {
        //     // new OpenFileVscCommandArgs
        //     {
        //         Uri = (filename),
        //     },
        // };

        Args = new object[]
            {
                 filename,
                2,
                2,
            };
    }

    public object[] Args { get; private set; }

    public class OpenFileVscCommandArgs
    {
        public required string Uri { get; init; }
    }
}