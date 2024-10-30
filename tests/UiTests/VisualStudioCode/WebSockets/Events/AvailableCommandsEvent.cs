namespace UiTests.VisualStudioCode.WebSockets.Events;

public class AvailableCommandsEvent
{
    public string Type { get; set; }

    public string[] Commands { get; set; } = [];
}