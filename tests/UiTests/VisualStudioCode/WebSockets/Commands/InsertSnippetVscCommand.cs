namespace UiTests.VisualStudioCode.WebSockets.Commands;

public sealed class InsertSnippetVscCommand : VscCommand
{
    public InsertSnippetVscCommand(string snippet)
        : base("editor.action.insertSnippet")
    {
        Args = new[]
        {
            new InsertSnippetVscCommandArgs
            {
                Snippet = snippet,
            },
        };
    }

    public InsertSnippetVscCommandArgs[] Args { get; private set; }

    public class InsertSnippetVscCommandArgs
    {
        public required string Snippet { get; init; }
    }
}