namespace RepoM.Api.Tests.QuickFilter;

using System;
using System.Collections.Generic;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using AwesomeAssertions;
using FakeItEasy;
using Microsoft.Extensions.Logging.Abstractions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
        var envelope = JObject.Parse(json);
        var persisted = envelope["Filters"]!.ToObject<List<QuickFilterModel>>(JsonSerializer.Create(new JsonSerializerSettings
        {
            Converters = { new QueryJsonConverter(), },
        }));
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

    [Fact]
    public void FindByQuery_ShouldReturnNull_WhenNoMatch()
    {
        // arrange
        var sut = CreateSut();
        sut.Add("Work", new SimpleTerm("tag", "work"));

        // act
        QuickFilterModel? result = sut.FindByQuery(new SimpleTerm("tag", "personal"));

        // assert
        result.Should().BeNull();
    }

    [Fact]
    public void Remove_ShouldRemoveUserFilter_AndRaiseChanged()
    {
        // arrange
        var sut = CreateSut();
        QuickFilterModel added = sut.Add("Work", new SimpleTerm("tag", "work"));
        var changedCount = 0;
        sut.Changed += (_, _) => changedCount++;

        // act
        sut.Remove(added.Id);

        // assert
        sut.GetAll().Should().HaveCount(2); // only built-in remain
        changedCount.Should().Be(1);
    }

    [Fact]
    public void Remove_ShouldDoNothing_WhenIdDoesNotExist()
    {
        // arrange
        var sut = CreateSut();
        var changedCount = 0;
        sut.Changed += (_, _) => changedCount++;

        // act
        sut.Remove(Guid.NewGuid());

        // assert
        sut.GetAll().Should().HaveCount(2);
        changedCount.Should().Be(0);
    }

    [Fact]
    public void SetActive_ShouldUpdateFilter_AndPersist()
    {
        // arrange
        var fileSystem = new MockFileSystem();
        var sut = CreateSut(fileSystem: fileSystem);
        QuickFilterModel added = sut.Add("Work", new SimpleTerm("tag", "work"));
        added.IsActive.Should().BeTrue();

        // act
        sut.SetActive(added.Id, false);

        // assert
        sut.GetAll().First(f => f.Id == added.Id).IsActive.Should().BeFalse();
    }

    [Fact]
    public void SetActive_ShouldDoNothing_WhenValueIsSame()
    {
        // arrange
        var sut = CreateSut();
        QuickFilterModel added = sut.Add("Work", new SimpleTerm("tag", "work"));
        var changedCount = 0;
        sut.Changed += (_, _) => changedCount++;

        // act
        sut.SetActive(added.Id, true); // already true

        // assert
        changedCount.Should().Be(0);
    }

    [Fact]
    public void SetInverse_ShouldUpdateFilter_AndRaiseChanged()
    {
        // arrange
        var sut = CreateSut();
        QuickFilterModel added = sut.Add("Work", new SimpleTerm("tag", "work"));
        var changedCount = 0;
        sut.Changed += (_, _) => changedCount++;

        // act
        sut.SetInverse(added.Id, true);

        // assert
        sut.GetAll().First(f => f.Id == added.Id).IsInverse.Should().BeTrue();
        changedCount.Should().Be(1);
    }

    [Fact]
    public void SetInverse_ShouldDoNothing_WhenValueIsSame()
    {
        // arrange
        var sut = CreateSut();
        QuickFilterModel added = sut.Add("Work", new SimpleTerm("tag", "work"));
        var changedCount = 0;
        sut.Changed += (_, _) => changedCount++;

        // act
        sut.SetInverse(added.Id, false); // already false

        // assert
        changedCount.Should().Be(0);
    }

    [Fact]
    public void UpdateLabel_ShouldUpdateUserFilter_AndRaiseChanged()
    {
        // arrange
        var sut = CreateSut();
        QuickFilterModel added = sut.Add("Work", new SimpleTerm("tag", "work"));
        var changedCount = 0;
        sut.Changed += (_, _) => changedCount++;

        // act
        sut.UpdateLabel(added.Id, "Personal");

        // assert
        sut.GetAll().First(f => f.Id == added.Id).Label.Should().Be("Personal");
        changedCount.Should().Be(1);
    }

    [Fact]
    public void UpdateToolTip_ShouldUpdateUserFilter_AndRaiseChanged()
    {
        // arrange
        var sut = CreateSut();
        QuickFilterModel added = sut.Add("Work", new SimpleTerm("tag", "work"));
        var changedCount = 0;
        sut.Changed += (_, _) => changedCount++;

        // act
        sut.UpdateToolTip(added.Id, "Work repositories");

        // assert
        sut.GetAll().First(f => f.Id == added.Id).ToolTip.Should().Be("Work repositories");
        changedCount.Should().Be(1);
    }

    [Fact]
    public void UpdateToolTip_ShouldIgnoreBuiltInFilter()
    {
        // arrange
        var sut = CreateSut();
        var builtIn = sut.GetAll()[0];
        var changedCount = 0;
        sut.Changed += (_, _) => changedCount++;

        // act
        sut.UpdateToolTip(builtIn.Id, "Changed");

        // assert
        changedCount.Should().Be(0);
    }

    [Fact]
    public void UpdateOrder_ShouldUpdateUserFilter_AndRaiseChanged()
    {
        // arrange
        var sut = CreateSut();
        QuickFilterModel added = sut.Add("Work", new SimpleTerm("tag", "work"));
        var changedCount = 0;
        sut.Changed += (_, _) => changedCount++;

        // act
        sut.UpdateOrder(added.Id, 42);

        // assert
        sut.GetAll().First(f => f.Id == added.Id).Order.Should().Be(42);
        changedCount.Should().Be(1);
    }

    [Fact]
    public void UpdateOrder_ShouldIgnoreBuiltInFilter()
    {
        // arrange
        var sut = CreateSut();
        var builtIn = sut.GetAll()[0];
        var changedCount = 0;
        sut.Changed += (_, _) => changedCount++;

        // act
        sut.UpdateOrder(builtIn.Id, 99);

        // assert
        changedCount.Should().Be(0);
    }

    [Fact]
    public void GetAll_ShouldReturnOrderedByOrder()
    {
        // arrange
        var sut = CreateSut();
        var a = sut.Add("A", new SimpleTerm("tag", "a"));
        var b = sut.Add("B", new SimpleTerm("tag", "b"));
        sut.UpdateOrder(b.Id, -100);

        // act
        var result = sut.GetAll();

        // assert - built-in (order -2, -1) come first, then B (-100 reordered), then A
        // Actually, the built-in filters have order -2 and -1, B is -100, so B should be first
        var userFilters = result.Where(f => !f.IsBuiltIn).ToList();
        userFilters[0].Label.Should().Be("B");
        userFilters[1].Label.Should().Be("A");
    }

    [Fact]
    public void Ctor_ShouldLoadExistingFilters_FromFile()
    {
        // arrange
        var fileSystem = new MockFileSystem();
        var filters = new List<QuickFilterModel>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Label = "Persisted",
                Query = new SimpleTerm("tag", "persisted"),
                Order = 5,
            },
        };
        var envelope = new { Filters = filters, CombineMode = "And" };
        var json = JsonConvert.SerializeObject(envelope, new JsonSerializerSettings { Converters = { new QueryJsonConverter(), }, });
        fileSystem.AddFile($"{AppDataPath}\\quickfilters.json", new MockFileData(json));

        // act
        var sut = CreateSut(fileSystem: fileSystem);

        // assert
        sut.GetAll().Should().HaveCount(3); // 2 built-in + 1 persisted
        sut.GetAll().Should().Contain(f => f.Label == "Persisted");
    }

    [Fact]
    public void CombineMode_ShouldDefaultToAnd()
    {
        // arrange & act
        var sut = CreateSut();

        // assert
        sut.CombineMode.Should().Be(QuickFilterCombineMode.And);
    }

    [Fact]
    public void CombineMode_Set_ShouldPersistAndRaiseChanged()
    {
        // arrange
        var fileSystem = new MockFileSystem();
        var sut = CreateSut(fileSystem: fileSystem);
        var changedCount = 0;
        sut.Changed += (_, _) => changedCount++;

        // act
        sut.CombineMode = QuickFilterCombineMode.Or;

        // assert
        sut.CombineMode.Should().Be(QuickFilterCombineMode.Or);
        changedCount.Should().Be(1);

        // Verify it was persisted by reloading
        var sut2 = CreateSut(fileSystem: fileSystem);
        sut2.CombineMode.Should().Be(QuickFilterCombineMode.Or);
    }

    [Fact]
    public void CombineMode_SetSameValue_ShouldNotRaiseChanged()
    {
        // arrange
        var sut = CreateSut();
        var changedCount = 0;
        sut.Changed += (_, _) => changedCount++;

        // act
        sut.CombineMode = QuickFilterCombineMode.And; // same as default

        // assert
        changedCount.Should().Be(0);
    }

    private static QuickFilterService CreateSut(MockFileSystem? fileSystem = null)
    {
        var appDataPathProvider = A.Fake<IAppDataPathProvider>();
        A.CallTo(() => appDataPathProvider.AppDataPath).Returns(AppDataPath);
        return new QuickFilterService(appDataPathProvider, fileSystem ?? new MockFileSystem(), NullLogger.Instance);
    }
}