namespace UiTests.VisualStudioCode.WebSockets;

using System;
using System.Net.WebSockets;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Xunit.Abstractions;

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
    private readonly Subject<string> _subject = new();
    private readonly Subject<CommandResponse> _subjectCommandResponse = new();
    private readonly Subject<bool> _focusSubject = new();
    private readonly ITestOutputHelper _outputHelper;
    private int _counter = 0;
    private readonly Uri _uri;
    private string[] _availableCommands;

    public VisualStudioWebSocketAutomation(Uri wsUri, ITestOutputHelper outputHelper)
    {
        _uri = wsUri ?? throw new ArgumentNullException(nameof(wsUri));
        _outputHelper = outputHelper ?? throw new ArgumentNullException(nameof(outputHelper));
        _client = new ClientWebSocket();
    }

    public ISubject<bool> FocusUpdated => _focusSubject;

    public async Task ConnectAsync(CancellationToken cancellationToken)
    {
        await _client.ConnectAsync(_uri, cancellationToken);
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
                        _outputHelper.WriteLine($"Received message: {json}");
                        _subject.OnNext(json);

                        string type = GetTypeFromBody(json);

                        switch (type)
                        {
                            case "command_response":
                                CommandResponse? commandResponse = JsonSerializer.Deserialize<CommandResponse>(json, DeserializeOptions);

                                if (commandResponse != null)
                                {
                                    _outputHelper.WriteLine($">>>> RESPONSE FOUND message: {commandResponse.Id} - {commandResponse.Type}");
                                    _subjectCommandResponse.OnNext(commandResponse);
                                }
                                break;

                            case "version":
                            case "debug":
                            case "git":
                            case "extensions":
                                // don't care
                                break;
                            case "focus":
                                FocusEvent focusEvent = Deserialize<FocusEvent>(json);
                                _focusSubject.OnNext(focusEvent.Focus);
                                break;
                            case "commands":
                                AvailableCommandsEvent availableCommandsEvent = Deserialize<AvailableCommandsEvent>(json);
                                _availableCommands = availableCommandsEvent.Commands;
                                break;
                            case "environment":
                                break;
                            case "workspace":
                                break;
                            case "editor":
                                EditorUpdateEvent editorUpdateEvent = Deserialize<EditorUpdateEvent>(json);
                                // todo
                                break;
                            case "error":
                                break;
                        }
                    }
                    catch (Exception e)
                    {
                        _subject.OnError(e);
                        _subjectCommandResponse.OnError(e);
                        stop = true;
                    }
                }
            }, cancellationToken);
    }

    private static T Deserialize<T>(string json)
    {
        return JsonSerializer.Deserialize<T>(json, DeserializeOptions)!;
    }

    private static string GetTypeFromBody(string body)
    {
        try
        {
            var jsonDoc = JsonDocument.Parse(body);
            JsonElement root = jsonDoc.RootElement;

            if (root.TryGetProperty("id", out JsonElement idProperty))
            {
                if (idProperty.TryGetInt32(out var id))
                {
                    return "command_response";
                }
            }

            string type = root.GetProperty("type").GetString();

            return type;
        }
        catch (Exception)
        {
            return "error";
        }
    }

    public async Task<string> ExecuteCommandAsync(MyCommand command)
    {
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
            await _client.SendAsync(message, WebSocketMessageType.Text, true, CancellationToken.None);
            result = await tcs.Task;
        }

        return result;
    }

    public async Task CloseAsync(CancellationToken ct)
    {
        await _client.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
    }

    public void Dispose()
    {
        _client.Dispose();
    }
}