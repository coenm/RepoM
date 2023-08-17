namespace RepoM.Api.Tests.IO.ModuleBasedRepositoryActionProvider;

using System;
using System.IO.Abstractions;
using System.Runtime.Caching;
using FakeItEasy;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using RepoM.Api.IO.ModuleBasedRepositoryActionProvider;
using RepoM.Api.IO.ModuleBasedRepositoryActionProvider.Deserialization;
using RepoM.Core.Plugin.Common;
using RepoM.Core.Plugin.Expressions;
using Xunit;

public class RepositoryConfigurationReaderTests
{
    [Fact]
    public void Ctor_ShouldThrow_WhenArgumentNull()
    {
        // arrange

        // act
        Func<RepositoryConfigurationReader> act1 = () => new RepositoryConfigurationReader(A.Dummy<IAppDataPathProvider>(), A.Dummy<IFileSystem>(), A.Dummy<IRepositoryActionDeserializer>(), A.Dummy<IRepositoryExpressionEvaluator>(), A.Dummy<ILogger>(), null!);
        Func<RepositoryConfigurationReader> act2 = () => new RepositoryConfigurationReader(A.Dummy<IAppDataPathProvider>(), A.Dummy<IFileSystem>(), A.Dummy<IRepositoryActionDeserializer>(), A.Dummy<IRepositoryExpressionEvaluator>(), null!, A.Dummy<ObjectCache>());
        Func<RepositoryConfigurationReader> act3 = () => new RepositoryConfigurationReader(A.Dummy<IAppDataPathProvider>(), A.Dummy<IFileSystem>(), A.Dummy<IRepositoryActionDeserializer>(), null!, A.Dummy<ILogger>(), A.Dummy<ObjectCache>());
        Func<RepositoryConfigurationReader> act4 = () => new RepositoryConfigurationReader(A.Dummy<IAppDataPathProvider>(), A.Dummy<IFileSystem>(), null!, A.Dummy<IRepositoryExpressionEvaluator>(), A.Dummy<ILogger>(), A.Dummy<ObjectCache>());
        Func<RepositoryConfigurationReader> act5 = () => new RepositoryConfigurationReader(A.Dummy<IAppDataPathProvider>(), null!, A.Dummy<IRepositoryActionDeserializer>(), A.Dummy<IRepositoryExpressionEvaluator>(), A.Dummy<ILogger>(), A.Dummy<ObjectCache>());
        Func<RepositoryConfigurationReader> act6 = () => new RepositoryConfigurationReader(null!, A.Dummy<IFileSystem>(), A.Dummy<IRepositoryActionDeserializer>(), A.Dummy<IRepositoryExpressionEvaluator>(), A.Dummy<ILogger>(), A.Dummy<ObjectCache>());

        // assert
        act1.Should().Throw<ArgumentNullException>();
        act2.Should().Throw<ArgumentNullException>();
        act3.Should().Throw<ArgumentNullException>();
        act4.Should().Throw<ArgumentNullException>();
        act5.Should().Throw<ArgumentNullException>();
        act6.Should().Throw<ArgumentNullException>();
    }
}