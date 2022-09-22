namespace RepoM.Ipc.Tests;

using System;
using System.Threading;
using System.Threading.Tasks;
using FakeItEasy;
using FluentAssertions;
using RepoM.Ipc;
using RepoM.Ipc.Tests.Internals;
using RepoM.Plugin.IpcService;
using VerifyXunit;
using Xunit;

[UsesVerify]
public class RepoMIpcClientTests : IDisposable
{
    private readonly IRepositorySource _repositorySource;
    private readonly IpcClient _client;
    private readonly IpcServer _server;

    public RepoMIpcClientTests()
    {
        _repositorySource = A.Fake<IRepositorySource>();
        var endpoint = new TestIpcEndpoint();
        _client = new IpcClient(endpoint);
        _server = new IpcServer(endpoint, _repositorySource);
    }

    public void Dispose()
    {
        _server.Stop();
        _server.Dispose();
    }

    [Fact]
    public void GetRepositories_SHouldReturnErrorMessage_WhenServerUnreachable()
    {
        // arrange
        _server.Stop();

        // act
        IpcClient.Result result = _client.GetRepositories();

        // assert
        result.Answer.Should().StartWith("RepoM seems"); // ... to be running but ... -> indicates an error
    }

    [Fact]
    public async Task GetMatchingRepositories_ShouldReturnExpected()
    {
        // arrange
        A.CallTo(() => _repositorySource.GetMatchingRepositories(A<string>._))
         .Returns(new Repository[]
             {
                 new Repository("N")
                     {
                         BranchWithStatus = "B",
                         Path = "P",
                     },
             });
        _server.Start();
        
        // act
        IpcClient.Result result = _client.GetRepositories();

        // assert
        await Verifier.Verify(result.Repositories);
    }

    [Fact]
    public void Stop_ShouldNotThrow_WhenAlreadyStopped()
    {
        // arrange
        _server.Start();
        // just to be sure, slee[p for two seconds. Normally, this is not okay but accepted for now.
        Thread.Sleep(2000);
        _server.Stop();

        // act
        Action act = () => _server.Stop();

        // assert
        act.Should().NotThrow();
    }
}