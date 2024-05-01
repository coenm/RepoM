namespace RepoM.Plugin.Heidi.Tests.Interface;

using System;
using System.Collections.Generic;
using FluentAssertions;
using RepoM.Plugin.Heidi.Interface;
using Xunit;

public class RepositoryHeidiConfigurationTests
{
    [Fact]
    public void Ctor_ShouldThrow_WhenArgumentIsNull()
    {
        // arrange
        string @string = "string";
        string[] array = [];
        HeidiDbConfig config = new HeidiDbConfig();

        // act
        var actions = new List<Action>
            {
                () => _ = new RepositoryHeidiConfiguration(@string, 1, array, @string, null!),
                () => _ = new RepositoryHeidiConfiguration(@string, 1, array, null!, config),
                () => _ = new RepositoryHeidiConfiguration(@string, 1, null!, @string, config),
                () => _ = new RepositoryHeidiConfiguration(null!, 1, array, @string, config),
            };

        // assert
        foreach (Action action in actions)
        {
            action.Should().Throw<ArgumentNullException>();
        }
    }
}