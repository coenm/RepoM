namespace RepoM.Ipc;

using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using NetMQ;
using NetMQ.Sockets;

public class IpcClient
{
    private string? _answer;
    private Repository[]? _repositories;

    public IpcClient(IIpcEndpoint endpointProvider)
    {
        EndpointProvider = endpointProvider ?? throw new ArgumentNullException(nameof(endpointProvider));
    }

    public Result GetRepositories()
    {
        return GetRepositories("list:.*");
    }

    public Result GetRepositories(string? query)
    {
        query ??= string.Empty;
        var watch = Stopwatch.StartNew();

        _answer = null;
        _repositories = null;

        using (var client = new RequestSocket())
        {
            client.Connect(EndpointProvider.Address);

            var load = Encoding.UTF8.GetBytes(query);
            client.SendFrame(load);
            client.ReceiveReady += ClientOnReceiveReady;

            client.Poll(TimeSpan.FromMilliseconds(3000));

            client.ReceiveReady -= ClientOnReceiveReady;
            client.Disconnect(EndpointProvider.Address);
        }

        watch.Stop();

        return new Result(
            _answer ?? GetErrorMessage(),
            watch.ElapsedMilliseconds,
            _repositories);
    }

    private string GetErrorMessage()
    {
        var isRepoMRunning = Process.GetProcessesByName("RepoM").Any();
        return isRepoMRunning
            ? $"RepoM seems to be running but does not answer.\nIt seems that it could not listen on {EndpointProvider.Address}.\nI don't know anything better than recommending a reboot. Sorry."
            : "RepoM seems not to be running :(";
    }

    private void ClientOnReceiveReady(object sender, NetMQSocketEventArgs e)
    {
        _answer = Encoding.UTF8.GetString(e.Socket.ReceiveFrameBytes());

        _repositories = _answer.Split(new string[] { Environment.NewLine, }, StringSplitOptions.None)
                                                         .Select(s => Repository.FromString(s))
                                                         .Where(r => r != null)
                                                         .Cast<Repository>()
                                                         .OrderBy(r => r.Name)
                                                         .ToArray();
    }

    public IIpcEndpoint EndpointProvider { get; }

    public class Result
    {
        public Result(string answer, long durationMilliseconds, Repository[]? repositories)
        {
            Answer = answer;
            DurationMilliseconds = durationMilliseconds;
            Repositories = repositories ?? Array.Empty<Repository>();
        }

        public string Answer { get; }

        public long DurationMilliseconds { get; }

        public Repository[] Repositories { get; }
    }
}