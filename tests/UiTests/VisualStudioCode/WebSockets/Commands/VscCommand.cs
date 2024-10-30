namespace UiTests.VisualStudioCode.WebSockets.Commands;

using System.Text.Json.Serialization;

public class VscCommand
{
    public VscCommand(string command)
    {
        Command = command;
    }

    [JsonPropertyOrder(-100)]
    public int Id { get; set; }

    [JsonPropertyOrder(-99)]
    public string Type => "command";

    [JsonPropertyOrder(-98)]
    public string Command { get; }
}