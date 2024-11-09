namespace UiTests.VisualStudioCode.WebSockets;

using System;
using System.Linq;
using System.Net.WebSockets;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using UiTests.VisualStudioCode.WebSockets.Commands;
using UiTests.VisualStudioCode.WebSockets.Events;
using Xunit.Abstractions;

// https://code.visualstudio.com/api
// https://marketplace.visualstudio.com/items?itemName=pascaldiehm.commandsocket
// https://marketplace.visualstudio.com/items?itemName=VscodePlugins-CmdKeys.vscode-plugins-websocket-commands
public class VisualStudioWebSocketAutomation : IDisposable
{
    public static readonly JsonSerializerOptions SerializeOptions = new(JsonSerializerDefaults.Web);
    public static readonly JsonSerializerOptions DeserializeOptions = new(JsonSerializerDefaults.Web)
    {
        // PropertyNameCaseInsensitive = true,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
    };

    private readonly ClientWebSocket _client;
    private Task _runningTask;
    private readonly ReplaySubject<CommandResponse> _subjectCommandResponse = new(1);
    private readonly ReplaySubject<bool> _focusSubject = new(1);
    private readonly ReplaySubject<EditorUpdateEvent> _editorUpdate = new(1);
    private readonly ITestOutputHelper _outputHelper;
    private int _counter = 0;
    private readonly Uri _uri;
    private string[] _availableCommands = [];

    public VisualStudioWebSocketAutomation(Uri wsUri, ITestOutputHelper outputHelper)
    {
        _uri = wsUri ?? throw new ArgumentNullException(nameof(wsUri));
        _outputHelper = outputHelper ?? throw new ArgumentNullException(nameof(outputHelper));
        _client = new ClientWebSocket();
        _client.Options.KeepAliveInterval = TimeSpan.FromMinutes(10);

    }

    public IObservable<bool> FocusUpdated => _focusSubject;

    public IObservable<EditorUpdateEvent> EditorUpdated => _editorUpdate;

    public async Task ConnectAsync(CancellationToken cancellationToken)
    {
        var success = false;
        while (!success)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                await _client.ConnectAsync(_uri, cancellationToken);
                success = true;
            }
            catch (Exception e)
            {
                await Task.Delay(200, cancellationToken);
            }
        }
    }

    public void StartProcessing(CancellationToken cancellationToken)
    {
        _runningTask = Task.Run(async () =>
            {
                bool stop = false;
                var buffer = new byte[1024];
                while (!stop || _client.State != WebSocketState.Closed)
                {
                    try
                    {
                        var endOfMessage = false;
                        var json = string.Empty;
                        while (!endOfMessage)
                        {
                            WebSocketReceiveResult result = await _client.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationToken);
                            json += Encoding.UTF8.GetString(buffer, 0, result.Count);
                            endOfMessage = result.EndOfMessage;
                        }

                        json = json.Trim();
                        if (string.IsNullOrEmpty(json))
                        {
                            continue;
                        }
                        
                        _outputHelper.WriteLine($"Received message: {json[..Math.Min(30, json.Length-1)]}");
  
                        var type = GetTypeFromBody(json);

                        switch (type)
                        {
                            case "command_response":
                                CommandResponse commandResponse = Deserialize<CommandResponse>(json);
                                _subjectCommandResponse.OnNext(commandResponse);
                                break;

                            case "focus":
                                FocusEvent focusEvent = Deserialize<FocusEvent>(json);
                                _focusSubject.OnNext(focusEvent.Focus);
                                break;

                            case "commands":
                                AvailableCommandsEvent availableCommandsEvent = Deserialize<AvailableCommandsEvent>(json);
                                _availableCommands = availableCommandsEvent.Commands;
                                break;

                            case "editor":
                                EditorUpdateEvent editorUpdateEvent = Deserialize<EditorUpdateEvent>(json);
                                _outputHelper.WriteLine($"Editor update:");
                                _outputHelper.WriteLine($" -  Path {editorUpdateEvent.Path}");
                                _outputHelper.WriteLine($" -  Name {editorUpdateEvent.Name}");
                                _outputHelper.WriteLine($" -  Line {editorUpdateEvent.Line}");
                                _outputHelper.WriteLine($" -  Lines {editorUpdateEvent.Lines}");
                                _outputHelper.WriteLine($" -  Column {editorUpdateEvent.Column}");
                                _outputHelper.WriteLine($" -  Indent {editorUpdateEvent.Indent}");
                                _outputHelper.WriteLine($" -  Tabs {editorUpdateEvent.Tabs}");
                                _editorUpdate.OnNext(editorUpdateEvent);
                                break;

                            case "error":
                                throw new Exception($"Could not pick type from {json}");
                                break;

                            case "version":
                            case "debug":
                            case "git":
                            case "extensions":
                            case "environment":
                            case "workspace":
                                // don't care
                                break;
                        }
                    }
                    catch (Exception e)
                    {
                        _subjectCommandResponse.OnError(e);
                        stop = true;
                    }
                }

                var x = _client.State;
            }, cancellationToken);
    }

    public Task<string> ExecuteCommandAsync(string command)
    {
        return ExecuteCommandAsync(new VscCommand(command));
    }

    public async Task<string> ExecuteCommandAsync<T>(T command) where T : VscCommand
    {
        if (_availableCommands.Length > 0 && !_availableCommands.Contains(command.Command))
        {
            // throw new Exception($"Command {command.Command} is not available in the list of available commands.");
        }

        if (command.Id == 0)
        {
            command.Id = Interlocked.Increment(ref _counter);
        }

        var msg = JsonSerializer.Serialize(command, SerializeOptions);
        var bytes = Encoding.UTF8.GetBytes(msg);
        var message = new ArraySegment<byte>(bytes);

        string result;

        var tcs = new TaskCompletionSource<string>();
        using (IDisposable registration = _subjectCommandResponse.Where(x => x.Id == command.Id).Subscribe(x => tcs.TrySetResult(x.Type)))
        {
            _outputHelper.WriteLine($"Sending {msg}");
            await _client.SendAsync(message, WebSocketMessageType.Text, true, CancellationToken.None);
            result = await tcs.Task;
        }

        return result;
    }

    public async Task CloseAsync(CancellationToken ct)
    {
        await _client.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
    }

    private static T Deserialize<T>(string json)
    {
        try
        {
            return JsonSerializer.Deserialize<T>(json, DeserializeOptions)!;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }

    }

    private static string GetTypeFromBody(string body)
    {
        try
        {
            var jsonDoc = JsonDocument.Parse(body);
            JsonElement root = jsonDoc.RootElement;

            if (root.TryGetProperty("id", out JsonElement idProperty) && idProperty.TryGetInt32(out var id))
            {
                return "command_response";
            }

            return root.GetProperty("type").GetString();
        }
        catch (Exception)
        {
            return "error";
        }
    }

    public void Dispose()
    {
        _client.Dispose();
    }
}