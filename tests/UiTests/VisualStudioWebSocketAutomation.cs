namespace UiTests;

using System;
using System.Net.WebSockets;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit.Abstractions;

public class VisualStudioWebSocketAutomation : IDisposable
{
    private readonly ClientWebSocket _client;
    private int _port;
    private Task _runningTask;
    private Subject<string> _subject = new Subject<string>();
    private ITestOutputHelper _outputHelper;

    public VisualStudioWebSocketAutomation(int port, ITestOutputHelper outputHelper)
    {
        _port = port;
        _outputHelper = outputHelper;
        _client = new ClientWebSocket();
    }

    public async Task<string> ExecuteCommandAsync(int id, string msg)
    {
        var bytes = Encoding.UTF8.GetBytes(msg);
        var message = new ArraySegment<byte>(bytes);

        string result = string.Empty;
        var tcs = new TaskCompletionSource<string>();
        using (IDisposable x = _subject.Where(x => x.Contains(id.ToString())).Subscribe(x => tcs.TrySetResult(x)))
        {
            await _client.SendAsync(message, WebSocketMessageType.Text, true, CancellationToken.None);
            result = await tcs.Task;
        }

        return result;
    }

    public async Task ConnectAsync(CancellationToken cancellationToken)
    {
        await _client.ConnectAsync(new Uri($"ws://localhost:{_port}"), cancellationToken);
    }

    public void StartProcessing(CancellationToken cancellationToken)
    {
        _runningTask = Task.Run(async () =>
            {
                var buffer = new byte[1024];
                while (_client.State != WebSocketState.Closed)
                {
                    try
                    {
                        WebSocketReceiveResult result = await _client.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationToken);
                        var receivedMessage = Encoding.UTF8.GetString(buffer, 0, result.Count);
                        Console.WriteLine($"Received message: {receivedMessage}");
                        _outputHelper.WriteLine($"Received message: {receivedMessage}");
                        _subject.OnNext(receivedMessage);
                    }
                    catch (Exception e)
                    {
                        //sskp
                    }
                }
            }, cancellationToken);
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