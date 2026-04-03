namespace RepoM.Api.Tests.QuickFilter;

using System;
using System.Collections.Generic;
using System.IO.Abstractions.TestingHelpers;
using AwesomeAssertions;
using FakeItEasy;
using Microsoft.Extensions.Logging.Abstractions;
using Newtonsoft.Json;
using RepoM.Api.QuickFilter;
using RepoM.Core.Plugin.Common;
using RepoM.Core.Plugin.RepositoryFiltering.Clause;
using RepoM.Core.Plugin.RepositoryFiltering.Clause.Terms;
using Xunit;

public class QuickFilterServiceTests
{
    private const string AppDataPath = @"C:\AppData\RepoM";

    [Fact]
    public void Ctor_ShouldThrow_WhenArgumentNull()
    {
        // arrange
        var pathProvider = A.Fake<IAppDataPathProvider>();
        var fileSystem = new MockFileSystem();

        // act
        Action act1 = () => _ = new QuickFilterService(pathProvider, fileSystem, null!);
        Action act2 = () => _ = new QuickFilterService(pathProvider, null!, NullLogger.Instance);
        Action act3 = () => _ = new QuickFilterService(null!, fileSystem, NullLogger.Instance);

        // assert
        act1.Should().ThrowExactly<ArgumentNullException>();
        act2.Should().ThrowExactly<ArgumentNullException>();
        act3.Should().ThrowExactly<ArgumentNullException>();
    }

    [Fact]
    public void GetAll_ShouldContainBuiltInFilters_WhenNoFileExists()
    {
        // arrange
        var sut = CreateSut();

        // act
        IReadOnlyList<QuickFilterModel> result = sut.GetAll();

        // assert
        result.Should().HaveCount(2);
        result[0].IsBuiltIn.Should().BeTrue();
        result[0].Label.Should().Be("\u2605");
        result[0].Query.ToString().Should().Be("is:favorite");
        result[1].IsBuiltIn.Should().BeTrue();
        result[1].Query.ToString().Should().Be("is:active");
    }

    [Fact]
    public void Add_ShouldPersistUserFilterOnly_AndRaiseChanged()
    {
        // arrange
        var fileSystem = new MockFileSystem();
        var sut = CreateSut(fileSystem: fileSystem);
        var changedCount = 0;
        sut.Changed += (_, _) => changedCount++;

        // act
        QuickFilterModel result = sut.Add("Work", new SimpleTerm("tag", "work"));

        // assert
        result.Label.Should().Be("Work");
        result.IsActive.Should().BeTrue();
        sut.GetAll().Should().HaveCount(3);
        changedCount.Should().Be(1);

        fileSystem.FileExists($"{AppDataPath}\\quickfilters.json").Should().BeTrue();
        var json = fileSystem.File.ReadAllText($"{AppDataPath}\\quickfilters.json");
        var persisted = JsonConvert.DeserializeObject<List<QuickFilterModel>>(json, new JsonSerializerSettings
        {
            Converters = { new QueryJsonConverter(), },
        });
        persisted.Should().HaveCount(1);
        persisted![0].Label.Should().Be("Work");
        persisted[0].Query.ToString().Should().Be("tag:work");
    }

    [Fact]
    public void Remove_ShouldIgnoreBuiltInFilter()
    {
        // arrange
        var sut = CreateSut();
        var builtInId = sut.GetAll()[0].Id;

        // act
        sut.Remove(builtInId);

        // assert
        sut.GetAll().Should().HaveCount(2);
    }

    [Fact]
    public void UpdateLabel_ShouldIgnoreBuiltInFilter()
    {
        // arrange
        var sut = CreateSut();
        var builtIn = sut.GetAll()[0];

        // act
        sut.UpdateLabel(builtIn.Id, "Changed");

        // assert
        sut.GetAll()[0].Label.Should().Be(builtIn.Label);
    }

    [Fact]
    public void FindByQuery_ShouldMatchCaseInsensitiveQueryString()
    {
        // arrange
        var sut = CreateSut();
        QuickFilterModel added = sut.Add("Work", new SimpleTerm("tag", "work"));

        // act
        QuickFilterModel? result = sut.FindByQuery(new SimpleTerm("TAG", "WORK"));

        // assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(added.Id);
    }

    private static QuickFilterService CreateSut(MockFileSystem? fileSystem = null)
    {
        var appDataPathProvider = A.Fake<IAppDataPathProvider>();
        A.CallTo(() => appDataPathProvider.AppDataPath).Returns(AppDataPath);
        return new QuickFilterService(appDataPathProvider, fileSystem ?? new MockFileSystem(), NullLogger.Instance);
    }
}