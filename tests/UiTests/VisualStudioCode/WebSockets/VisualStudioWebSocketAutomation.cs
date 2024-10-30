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
    public static readonly JsonSerializerOptions _serializeOptions = new(JsonSerializerDefaults.Web);
    public static readonly JsonSerializerOptions _deserializeOptions = new(JsonSerializerDefaults.Web)
    {
        // PropertyNameCaseInsensitive = true,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
    };

    private readonly ClientWebSocket _client;
    private int _port;
    private Task _runningTask;
    private Subject<string> _subject = new Subject<string>();
    private Subject<CommandResponse> _subjectReponse = new Subject<CommandResponse>();
    private ITestOutputHelper _outputHelper;
    private int _counter = 0;

    public VisualStudioWebSocketAutomation(int port, ITestOutputHelper outputHelper)
    {
        _port = port;
        _outputHelper = outputHelper;
        _client = new ClientWebSocket();
    }

    public async Task ConnectAsync(CancellationToken cancellationToken)
    {
        await _client.ConnectAsync(new Uri($"ws://localhost:{_port}"), cancellationToken);
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
                        WebSocketReceiveResult result = await _client.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationToken);
                        var receivedMessage = Encoding.UTF8.GetString(buffer, 0, result.Count);
                        Console.WriteLine($"Received message: {receivedMessage}");
                        _outputHelper.WriteLine($"Received message: {receivedMessage}");
                        _subject.OnNext(receivedMessage);

                        try
                        {
                            CommandResponse? commandResponse = JsonSerializer.Deserialize<CommandResponse>(receivedMessage.Trim(), _deserializeOptions);

                            if (commandResponse != null)
                            {
                                _outputHelper.WriteLine($">>>> RESPONSE FOUND message: {commandResponse.Id} - {commandResponse.Type}");
                                _subjectReponse.OnNext(commandResponse);
                            }
                        }
                        catch (Exception e)
                        {
                            _outputHelper.WriteLine("Error when deserializing " + e.Message);
                        }

                    }
                    catch (Exception e)
                    {
                        _subject.OnError(e);
                        _subjectReponse.OnError(e);
                        stop = true;
                    }
                }
            }, cancellationToken);
    }

    public async Task<string> ExecuteCommandAsync(MyCommand command)
    {
        if (command.Id == 0)
        {
            command.Id = Interlocked.Increment(ref _counter);
        }

        var msg = System.Text.Json.JsonSerializer.Serialize(command, _serializeOptions);
        var bytes = Encoding.UTF8.GetBytes(msg);
        var message = new ArraySegment<byte>(bytes);

        string result;

        var tcs = new TaskCompletionSource<string>();
        using (IDisposable x = _subjectReponse.Where(x => x.Id == command.Id).Subscribe(x => tcs.TrySetResult(x.Type)))
        {
            await _client.SendAsync(message, WebSocketMessageType.Text, true, CancellationToken.None);
            result = await tcs.Task;
        }

        return result;
    }

    public void Dispose()
    {
        _client.Dispose();
    }

    public async Task CloseAsync(CancellationToken ct)
    {
        await _client.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
    }
}