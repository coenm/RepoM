namespace RepoM.Api.Tests.Git;

using System;
using System.IO.Abstractions;
using System.Linq;
using System.Reflection;
using FakeItEasy;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using RepoM.Api.Git;
using RepoM.Api.Git.AutoFetch;
using RepoM.Api.IO;
using Xunit;

public class DefaultRepositoryMonitorTests
{
    [Fact]
    public void Ctor_ShouldThrow_WhenArgumentNull()
    {
        // arrange
        IPathProvider pathProvider = A.Dummy<IPathProvider>();
        IRepositoryReader repositoryReader = A.Dummy<IRepositoryReader>();
        IRepositoryDetectorFactory repositoryDetectorFactory = A.Dummy<IRepositoryDetectorFactory>();
        IRepositoryObserverFactory repositoryObserverFactory = A.Dummy<IRepositoryObserverFactory>();
        IGitRepositoryFinderFactory gitRepositoryFinderFactory = A.Dummy<IGitRepositoryFinderFactory>();
        IRepositoryStore repositoryStore = A.Dummy<IRepositoryStore>();
        IRepositoryInformationAggregator repositoryInformationAggregator = A.Dummy<IRepositoryInformationAggregator>();
        IAutoFetchHandler autoFetchHandler = A.Dummy<IAutoFetchHandler>();
        IRepositoryIgnoreStore repositoryIgnoreStore = A.Dummy<IRepositoryIgnoreStore>();
        IFileSystem fileSystem = A.Dummy<IFileSystem>();
        ILogger logger = A.Dummy<ILogger>();

        ConstructorInfo ctor = typeof(DefaultRepositoryMonitor).GetConstructors().Single();

        var parameters = new object[]
            {
                pathProvider,
                repositoryReader,
                repositoryDetectorFactory,
                repositoryObserverFactory,
                gitRepositoryFinderFactory,
                repositoryStore,
                repositoryInformationAggregator,
                autoFetchHandler,
                repositoryIgnoreStore,
                fileSystem,
                logger,
            };

        var result = ctor.Invoke(parameters) as DefaultRepositoryMonitor;
        result.Should().NotBeNull();

        // act
        for (int i = 0; i < parameters.Length; i++)
        {
            parameters[i] = null!;
            Action act = () =>
                {
                    try
                    {
                        ctor.Invoke(parameters);
                    }
                    catch (TargetInvocationException e)
                    {
                        throw e.InnerException ?? e;
                    }
                };

            // assert
            act.Should().Throw<ArgumentNullException>();
        }
    }
}