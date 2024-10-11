namespace RepoM.ActionMenu.Core.Tests.Abstractions;

using System.Collections.Generic;
using FluentAssertions;
using RepoM.ActionMenu.Core.Abstractions;
using Xunit;

public class SystemEnvironmentsTests
{
    [Fact]
    public void GetEnvironmentVariables_ShouldReturnEnvironmentVariables()
    {
        // arrange
        SystemEnvironments sut = SystemEnvironments.Instance;

        // act
        Dictionary<string, string> result = sut.GetEnvironmentVariables();

        // assert
        result.Should().NotBeNull();
    }
}