namespace UiTests.VisualStudioCode.WebSockets;

public class CommandResponse
{
    public int Id { get; set; }

    public string Type { get; set; }
}

public class EditorUpdateEvent
{
    public string Type { get; set; }
    public string Name { get; set; }
    public string Path { get; set; }
    public string Language { get; set; }
    public string Encoding { get; set; }
    public string Eol { get; set; }
    public int Indent { get; set; }
    public bool Tabs { get; set; }
    public bool Dirty { get; set; }
    public int Column { get; set; }
    public int Line { get; set; }
    public int Lines { get; set; }
    public int Warnings { get; set; }
    public int Errors { get; set; }
}



public class FocusEvent
{
    public string Type { get; set; }

    public bool Focus { get; set; }
}

public class AvailableCommandsEvent
{
    public string Type { get; set; }

    public string[] Commands { get; set; } = [];
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
