namespace RepoM.Ipc.Tests;

using FluentAssertions;
using RepoM.Ipc;
using Xunit;

public class RepositoryTests
{
    [Fact]
    public void Deserializes_With_Three_Arguments()
    {
        // arrange

        // act
        var result = Repository.FromString("Name::Branch::Path")!;

        // assert
        result.Name.Should().Be("Name");
        result.BranchWithStatus.Should().Be("Branch");
        result.Path.Should().Be("Path");
        result.HasUnpushedChanges.Should().BeFalse();
    }

    [Fact]
    public void Deserializes_With_Four_Arguments()
    {
        // arrange

        // act
        var result = Repository.FromString("Name::Branch::Path::1")!;

        // assert
        result.Name.Should().Be("Name");
        result.BranchWithStatus.Should().Be("Branch");
        result.Path.Should().Be("Path");
        result.HasUnpushedChanges.Should().BeTrue();
    }

    [Fact]
    public void Returns_Null_For_Less_Than_Three_Arguments()
    {
        // arrange

        // act
        var result = Repository.FromString("Name::Branch");

        // assert
        result.Should().BeNull();
    }

    [Fact]
    public void Returns_Null_For_Too_Much_Arguments()
    {
        // arrange

        // act
        var result = Repository.FromString("Name::Branch::Path::1::Mode");

        // assert
        result.Should().BeNull();
    }

    [Fact]
    public void Serializes_With_Four_Arguments()
    {
        // arrange

        // act
        var result = new Repository("N")
            {
                BranchWithStatus = "B",
                Path = "P",
            };

        // assert
        result.ToString().Should().Be("N::B::P::0"); // 0 is "HasUnpushedChanges"
    }

    [Fact]
    public void Returns_Null_For_Less_Than_Mandatory_Arguments()
    {
        // arrange

        // act
        var result = new Repository("N") { BranchWithStatus = "B", };

        // assert
        result.ToString().Should().Be("");
    }

    [Fact]
    public void Replaces_Backslashes_With_Slashes()
    {
        // arrange

        // act
        var result = Repository.FromString("Name::Branch::C:\\Users\\doe")!;

        // assert
        result.SafePath.Should().Be("C:/Users/doe");
    }

    [Fact]
    public void Returns_Empty_String_For_Null_Path()
    {
        // arrange

        // act
        var result = Repository.FromString("Name::Branch::C:\\Users\\doe")!;

        // assert
        result.Path = null!;
        result.SafePath.Should().Be("");
    }
}
