namespace RepoM.ActionMenu.Core.Tests.ConfigReader;

using System;
using System.Runtime.Caching;
using FluentAssertions;
using JetBrains.Annotations;
using RepoM.ActionMenu.Core.ConfigReader;
using Xunit;

public class FileStoreTests
{
    [Fact]
    public void Ctor_ShouldThrow_WhenArgumentNull()
    {
        // arrange

        // act
        Func<FileStore<DummyClass>> act1 = () => new FileStore<DummyClass>(null!);

        // assert
        act1.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Get_ShouldReturnNull_WhenNotExists()
    {
        // arrange
        var cache = new MemoryCache($"{nameof(FileStoreTests)}-{nameof(Get_ShouldReturnNull_WhenNotExists)}");
        var sut = new FileStore<DummyClass>(cache);

        // act
        DummyClass? result = sut.Get("x.txt");

        // assert
        result.Should().BeNull();
        cache.GetCount().Should().Be(0);
    }
}

[UsedImplicitly]
public sealed record DummyClass
{
}