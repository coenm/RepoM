namespace UiTests.VisualStudioCode.WebSockets;

public class CommandResponse
{
    public int Id { get; set; }

    public string Type { get; set; }
}

public class MyCommand
{
    public int Id { get; set; }

    public string Type => "command";

    public required string Command { get; init; }
}

public static class Commands
{
    public static class WorkBench
    {
        public static class Action
        {
            public const string GoToLine = "workbench.action.gotoLine";
        }
    }
}
