namespace Specs.Ipc;

using NUnit.Framework;
using Moq;
using FluentAssertions;
using RepoM.Ipc;
using RepoM.Plugin.IpcService;

public class RepoZIpcClientTests
{
    private readonly Mock<IRepositorySource> _repositorySource = new Mock<IRepositorySource>();
    private IpcClient _client = null!;
    private IpcServer _server = null!;

    [SetUp]
    public void Setup()
    {
        var endpoint = new TestIpcEndpoint();

        _client = new IpcClient(endpoint);
        _server = new IpcServer(endpoint, _repositorySource.Object);
    }

    public class GetRepositoriesMethod : RepoZIpcClientTests
    {
        [Test]
        public void Returns_An_Error_Message_If_RepoZ_Is_Not_Reachable()
        {
            _server.Stop();

            IpcClient.Result result = _client.GetRepositories();
            result.Answer.Should().StartWith("RepoM seems"); // ... to be running but ... -> indicates an error
        }

        [Test]
        public void Returns_Deserialized_Matching_Repositories()
        {
            _server.Start();
            _repositorySource
                .Setup(rs => rs.GetMatchingRepositories(It.IsAny<string>()))
                .Returns(new Repository[]
                    {
                        new Repository("N")
                            {
                                BranchWithStatus = "B",
                                Path = "P",
                            },
                    });

            IpcClient.Result result = _client.GetRepositories();

            _server.Stop();

            result.Repositories.Should().HaveCount(1);
        }
    }
}