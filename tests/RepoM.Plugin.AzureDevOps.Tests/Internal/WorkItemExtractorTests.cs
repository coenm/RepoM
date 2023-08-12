namespace RepoM.Plugin.AzureDevOps.Tests.Internal;

using System;
using FluentAssertions;
using Xunit;
using Sut = AzureDevOps.Internal.WorkItemExtractor;

public class WorkItemExtractorTests
{
    [Fact]
    public void GetDistinctWorkItemsFromCommitMessages_ShouldReturnEmptyResultSet_WhenInputIsEmpty()
    {
        // arrange

        // act
        var result = Sut.GetDistinctWorkItemsFromCommitMessages(Array.Empty<string>());

        // assert
        result.Should().BeEmpty();
    }

    [Theory]
    [InlineData("abc #xx")]
    [InlineData("abc # 1 x")]
    public void GetDistinctWorkItemsFromCommitMessages_ShouldReturnEmptySet_WhenInputIs(string input)
    {
        // arrange

        // act
        var result = Sut.GetDistinctWorkItemsFromCommitMessages(new[] { input, });

        // assert
        result.Should().BeEmpty();
    }

    [Theory]
    [InlineData("abc #13 xx","13")]
    [InlineData("abc #13xx","13")] // this should return false.
    [InlineData("abc#13 xx","13")] // this should return false.
    [InlineData("#13","13")]
    [InlineData(" #13","13")]
    [InlineData("#13 ","13")]
    [InlineData("#13 xx #13 ", "13")] //distinct
    public void GetDistinctWorkItemsFromCommitMessages_ShouldReturnExpected_WhenInputIs(string input, string expectedOutput)
    {
        // arrange

        // act
        var result = Sut.GetDistinctWorkItemsFromCommitMessages(new[] { input, });

        // assert
        result.Should().BeEquivalentTo(expectedOutput);
    }

    [Fact]
    public void GetDistinctWorkItemsFromCommitMessages_ShouldHandleMultipleCommitMessages()
    {
        // arrange
        var messages = new[]
            {
                string.Empty,
                "Message 12 without hashtag",
                "Message 14 without #",
                "Message 15 without #",
                "Solved issue #34",
                "Solved issue #34 again",
                "Solved issue #44 with" + Environment.NewLine + "Multi line commit",
            };

        // act
        var result = Sut.GetDistinctWorkItemsFromCommitMessages(messages);

        // assert
        result.Should().BeEquivalentTo("34", "44");
    }
}