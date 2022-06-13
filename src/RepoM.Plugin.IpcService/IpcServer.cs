namespace RepoM.Plugin.IpcService;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NetMQ;
using NetMQ.Sockets;
using RepoM.Ipc;

internal class IpcServer : IDisposable
{
    private ResponseSocket? _socketServer;
    private readonly IIpcEndpoint _endpointProvider;
    private readonly IRepositorySource _repositorySource;

    public IpcServer(IIpcEndpoint endpointProvider, IRepositorySource repositorySource)
    {
        _endpointProvider = endpointProvider ?? throw new ArgumentNullException(nameof(endpointProvider));
        _repositorySource = repositorySource ?? throw new ArgumentNullException(nameof(repositorySource));
    }

    public void Start()
    {
        Task.Run(() => StartInternal());
    }

    private void StartInternal()
    {
        _socketServer = new ResponseSocket(_endpointProvider.Address);

        while (true)
        {
            var load = _socketServer.ReceiveFrameBytes(out var hasMore);

            var message = Encoding.UTF8.GetString(load);

            if (string.IsNullOrEmpty(message))
            {
                return;
            }

            if (message.StartsWith("list:", StringComparison.Ordinal))
            {
                var repositoryNamePattern = message.Substring("list:".Length);

                var answer = "(no repositories found)";
                try
                {
                    Repository[] repos = _repositorySource.GetMatchingRepositories(repositoryNamePattern);
                    if (repos.Any())
                    {
                        IEnumerable<string> serializedRepositories = repos
                                                                     .Where(r => r != null)
                                                                     .Select(r => r.ToString());

                        answer = string.Join(Environment.NewLine, serializedRepositories);
                    }
                }
                catch (Exception ex)
                {
                    answer = ex.Message;
                }

                _socketServer.SendFrame(Encoding.UTF8.GetBytes(answer));
            }

            Thread.Sleep(100);
        }
    }

    public void Stop()
    {
        Dispose();
    }

    public void Dispose()
    {
        _socketServer?.Disconnect(_endpointProvider.Address);
        _socketServer?.Dispose();
        _socketServer = null;
    }
}