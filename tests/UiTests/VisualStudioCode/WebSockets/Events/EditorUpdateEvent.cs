namespace UiTests.VisualStudioCode.WebSockets.Events;

public class EditorUpdateEvent
{
    public string? Type { get; set; }

    public string? Name { get; set; }

    public string? Path { get; set; }

    public string? Language { get; set; }

    public string? Encoding { get; set; }

    public string? Eol { get; set; }

    public int? Indent { get; set; }

    public bool? Tabs { get; set; }

    public bool? Dirty { get; set; }

    public int? Column { get; set; }

    public int? Line { get; set; }

    public int? Lines { get; set; }

    public int? Warnings { get; set; }

    public int? Errors { get; set; }
}