namespace UiTests.VisualStudioCode.WebSockets.Commands;

public class VscCommand
{
    public VscCommand(string command)
    {
        Command = command;
    }

    public int Id { get; set; }

    public string Type => "command";

    public string Command { get; }
}