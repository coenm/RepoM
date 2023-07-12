namespace RepoM.Plugin.AzureDevOps.Tests.Internal;

using System;
using FakeItEasy;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using RepoM.Api.Common;
using RepoM.Plugin.AzureDevOps.Internal;
using Xunit;

public class AzureDevOpsPullRequestServiceTests
{
    [Fact]
    public void Ctor_ShouldThrow_WhenArgumentNull()
    {
        // arrange

        // act
        Func<AzureDevOpsPullRequestService> act1 = () => new AzureDevOpsPullRequestService(A.Dummy<IAppSettingsService>(), null!);
        Func<AzureDevOpsPullRequestService> act2 = () => new AzureDevOpsPullRequestService(null!, A.Dummy<ILogger>());

        // assert
        act1.Should().Throw<ArgumentNullException>();
        act2.Should().Throw<ArgumentNullException>();
    }
}