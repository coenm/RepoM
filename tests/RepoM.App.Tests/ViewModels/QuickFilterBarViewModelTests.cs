namespace RepoM.App.Tests.ViewModels;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using AwesomeAssertions;
using FakeItEasy;
using Microsoft.Extensions.Logging.Abstractions;
using RepoM.Api.QuickFilter;
using RepoM.App.RepositoryFiltering;
using RepoM.App.ViewModels;
using RepoM.Core.Plugin.RepositoryFiltering;
using RepoM.Core.Plugin.RepositoryFiltering.Clause;
using RepoM.Core.Plugin.RepositoryFiltering.Clause.Terms;
using Xunit;

public class QuickFilterBarViewModelTests
{
    [Fact]
    public void Ctor_ShouldThrow_WhenArgumentNull()
    {
        // arrange
        var service = new TestQuickFilterService();
        var repositoryFilteringManager = A.Fake<IRepositoryFilteringManager>();
        var queryParsers = new[] { new TestNamedQueryParser("Default", text => new FreeText(text)), };

        // act
        Action act1 = () => _ = new QuickFilterBarViewModel(null!, repositoryFilteringManager, queryParsers, NullLogger.Instance);
        Action act2 = () => _ = new QuickFilterBarViewModel(service, null!, queryParsers, NullLogger.Instance);
        Action act3 = () => _ = new QuickFilterBarViewModel(service, repositoryFilteringManager, null!, NullLogger.Instance);
        Action act4 = () => _ = new QuickFilterBarViewModel(service, repositoryFilteringManager, queryParsers, null!);

        // assert
        act1.Should().ThrowExactly<ArgumentNullException>();
        act2.Should().ThrowExactly<ArgumentNullException>();
        act3.Should().ThrowExactly<ArgumentNullException>();
        act4.Should().ThrowExactly<ArgumentNullException>();
    }

    [Fact]
    public void Ctor_ShouldThrow_WhenNoQueryParsersAvailable()
    {
        // arrange
        var service = new TestQuickFilterService();
        var repositoryFilteringManager = A.Fake<IRepositoryFilteringManager>();

        // act
        Action act = () => _ = new QuickFilterBarViewModel(service, repositoryFilteringManager, [], NullLogger.Instance);

        // assert
        act.Should().ThrowExactly<ArgumentException>()
            .WithMessage("At least one query parser must be available.*");
    }

    [Fact]
    public void AddFromTag_ShouldActivateExistingFilter_WhenQueryAlreadyExists()
    {
        // arrange
        var service = new TestQuickFilterService(
        [
            new QuickFilterModel
            {
                Id = Guid.NewGuid(),
                Label = "work",
                Query = new SimpleTerm("tag", "work"),
                IsActive = false,
            },
        ]);
        var sut = CreateSut(service);

        // act
        sut.AddFromTag("work");

        // assert
        service.SetActiveCalls.Should().ContainSingle();
        service.AddCalls.Should().BeEmpty();
        service.Filters[0].IsActive.Should().BeTrue();
    }

    [Fact]
    public void SaveSearchTextCommand_ShouldParseAndAddUsingSelectedParser()
    {
        // arrange
        var service = new TestQuickFilterService();
        var selectedParser = new TestNamedQueryParser("Lucene", text => new SimpleTerm("branch", text));
        var defaultParser = new TestNamedQueryParser("Default", text => new FreeText(text));
        var repositoryFilteringManager = A.Fake<IRepositoryFilteringManager>();
        A.CallTo(() => repositoryFilteringManager.SelectedQueryParserKey).Returns("Lucene");
        var sut = new QuickFilterBarViewModel(service, repositoryFilteringManager, [defaultParser, selectedParser], NullLogger.Instance);

        // act
        sut.SaveSearchTextCommand.Execute("  main  ");

        // assert
        service.AddCalls.Should().ContainSingle();
        service.AddCalls[0].label.Should().Be("main");
        service.AddCalls[0].query.Should().BeOfType<SimpleTerm>();
        ((SimpleTerm)service.AddCalls[0].query).Term.Should().Be("branch");
        ((SimpleTerm)service.AddCalls[0].query).Value.Should().Be("main");
    }

    [Fact]
    public void GetCombinedActiveQuery_ShouldCombineActiveAndInverseQueries()
    {
        // arrange
        var service = new TestQuickFilterService(
        [
            new QuickFilterModel { Id = Guid.NewGuid(), Label = "work", Query = new SimpleTerm("tag", "work"), IsActive = true },
            new QuickFilterModel { Id = Guid.NewGuid(), Label = "archived", Query = new SimpleTerm("tag", "archived"), IsActive = true, IsInverse = true },
            new QuickFilterModel { Id = Guid.NewGuid(), Label = "ignored", Query = new SimpleTerm("tag", "ignored"), IsActive = false },
        ]);
        var sut = CreateSut(service);

        // act
        IQuery? result = sut.GetCombinedActiveQuery();

        // assert
        result.Should().BeOfType<AndQuery>();
        result!.ToString().Should().Be("And(tag:work, Not(tag:archived))");
    }

    [Fact]
    public void GetCombinedActiveQuery_ShouldUseOrQuery_WhenCombineModeIsOr()
    {
        // arrange
        var service = new TestQuickFilterService(
        [
            new QuickFilterModel { Id = Guid.NewGuid(), Label = "work", Query = new SimpleTerm("tag", "work"), IsActive = true },
            new QuickFilterModel { Id = Guid.NewGuid(), Label = "personal", Query = new SimpleTerm("tag", "personal"), IsActive = true },
        ]);
        var sut = CreateSut(service);
        sut.CombineMode = QuickFilterCombineMode.Or;

        // act
        IQuery? result = sut.GetCombinedActiveQuery();

        // assert
        result.Should().BeOfType<OrQuery>();
        result!.ToString().Should().Be("Or(tag:work, tag:personal)");
    }

    [Fact]
    public void ToggleCombineMode_ShouldSwitchBetweenAndAndOr()
    {
        // arrange
        var sut = CreateSut(new TestQuickFilterService());

        // act & assert
        sut.CombineMode.Should().Be(QuickFilterCombineMode.And);
        sut.CombineModeLabel.Should().Be("AND");

        sut.ToggleCombineMode();
        sut.CombineMode.Should().Be(QuickFilterCombineMode.Or);
        sut.CombineModeLabel.Should().Be("OR");

        sut.ToggleCombineMode();
        sut.CombineMode.Should().Be(QuickFilterCombineMode.And);
        sut.CombineModeLabel.Should().Be("AND");
    }

    [Fact]
    public void ToggleCombineMode_ShouldRaiseFilterStateChanged()
    {
        // arrange
        var sut = CreateSut(new TestQuickFilterService());
        var filterStateChangedCount = 0;
        var propertyNames = new List<string?>();
        sut.FilterStateChanged += (_, _) => filterStateChangedCount++;
        sut.PropertyChanged += (_, e) => propertyNames.Add(e.PropertyName);

        // act
        sut.ToggleCombineMode();

        // assert
        filterStateChangedCount.Should().Be(1);
        propertyNames.Should().Contain(nameof(QuickFilterBarViewModel.CombineMode));
        propertyNames.Should().Contain(nameof(QuickFilterBarViewModel.CombineModeLabel));
        propertyNames.Should().Contain(nameof(QuickFilterBarViewModel.CombineModeToolTip));
    }

    [Fact]
    public void CombineMode_ShouldDefaultToAnd()
    {
        // arrange & act
        var sut = CreateSut(new TestQuickFilterService());

        // assert
        sut.CombineMode.Should().Be(QuickFilterCombineMode.And);
        sut.CombineModeLabel.Should().Be("AND");
    }

    [Fact]
    public void CombineMode_SetSameValue_ShouldNotRaiseEvents()
    {
        // arrange
        var sut = CreateSut(new TestQuickFilterService());
        var filterStateChangedCount = 0;
        var propertyChangedCount = 0;
        sut.FilterStateChanged += (_, _) => filterStateChangedCount++;
        sut.PropertyChanged += (_, _) => propertyChangedCount++;

        // act
        sut.CombineMode = QuickFilterCombineMode.And; // same as default

        // assert
        filterStateChangedCount.Should().Be(0);
        propertyChangedCount.Should().Be(0);
    }

    [Fact]
    public void CombineModeToolTip_ShouldDescribeAndMode_WhenAnd()
    {
        // arrange
        var sut = CreateSut(new TestQuickFilterService());

        // act & assert
        sut.CombineModeToolTip.Should().Contain("AND");
        sut.CombineModeToolTip.Should().Contain("all must match");
    }

    [Fact]
    public void CombineModeToolTip_ShouldDescribeOrMode_WhenOr()
    {
        // arrange
        var sut = CreateSut(new TestQuickFilterService());
        sut.CombineMode = QuickFilterCombineMode.Or;

        // act & assert
        sut.CombineModeToolTip.Should().Contain("OR");
        sut.CombineModeToolTip.Should().Contain("any must match");
    }

    [Fact]
    public void GetCombinedActiveQuery_ShouldReturnNull_WhenNoActiveFilters_RegardlessOfMode()
    {
        // arrange
        var service = new TestQuickFilterService(
        [
            new QuickFilterModel { Id = Guid.NewGuid(), Label = "work", Query = new SimpleTerm("tag", "work"), IsActive = false },
        ]);
        var sut = CreateSut(service);

        // act & assert (AND)
        sut.GetCombinedActiveQuery().Should().BeNull();

        // act & assert (OR)
        sut.CombineMode = QuickFilterCombineMode.Or;
        sut.GetCombinedActiveQuery().Should().BeNull();
    }

    [Fact]
    public void GetCombinedActiveQuery_SingleActive_ShouldReturnQueryDirectly_RegardlessOfMode()
    {
        // arrange
        var service = new TestQuickFilterService(
        [
            new QuickFilterModel { Id = Guid.NewGuid(), Label = "work", Query = new SimpleTerm("tag", "work"), IsActive = true },
        ]);
        var sut = CreateSut(service);

        // act & assert (AND mode - single filter returns raw query, not wrapped in AndQuery)
        var resultAnd = sut.GetCombinedActiveQuery();
        resultAnd.Should().BeOfType<SimpleTerm>();

        // act & assert (OR mode - single filter returns raw query, not wrapped in OrQuery)
        sut.CombineMode = QuickFilterCombineMode.Or;
        var resultOr = sut.GetCombinedActiveQuery();
        resultOr.Should().BeOfType<SimpleTerm>();
    }

    [Fact]
    public void GetCombinedActiveQuery_OrMode_ShouldWrapInverseFiltersWithNotQuery()
    {
        // arrange
        var service = new TestQuickFilterService(
        [
            new QuickFilterModel { Id = Guid.NewGuid(), Label = "work", Query = new SimpleTerm("tag", "work"), IsActive = true },
            new QuickFilterModel { Id = Guid.NewGuid(), Label = "archived", Query = new SimpleTerm("tag", "archived"), IsActive = true, IsInverse = true },
        ]);
        var sut = CreateSut(service);
        sut.CombineMode = QuickFilterCombineMode.Or;

        // act
        IQuery? result = sut.GetCombinedActiveQuery();

        // assert
        result.Should().BeOfType<OrQuery>();
        result!.ToString().Should().Be("Or(tag:work, Not(tag:archived))");
    }

    [Fact]
    public void ToggleCombineModeCommand_ShouldToggleMode()
    {
        // arrange
        var sut = CreateSut(new TestQuickFilterService());
        sut.CombineMode.Should().Be(QuickFilterCombineMode.And);

        // act
        sut.ToggleCombineModeCommand.Execute(null);

        // assert
        sut.CombineMode.Should().Be(QuickFilterCombineMode.Or);
    }

    [Fact]
    public void ServiceChanged_ShouldRefreshItems_AndRaiseNotifications()
    {
        // arrange
        var service = new TestQuickFilterService();
        var sut = CreateSut(service);
        var propertyNames = new List<string?>();
        var filterStateChangedCount = 0;
        sut.PropertyChanged += (_, e) => propertyNames.Add(e.PropertyName);
        sut.FilterStateChanged += (_, _) => filterStateChangedCount++;

        // act
        service.Filters.Add(new QuickFilterModel { Id = Guid.NewGuid(), Label = "Work", Query = new SimpleTerm("tag", "work") });
        service.RaiseChanged();

        // assert
        sut.Items.Should().HaveCount(1);
        sut.HasItems.Should().BeTrue();
        propertyNames.Should().Contain(nameof(QuickFilterBarViewModel.HasItems));
        filterStateChangedCount.Should().Be(1);
    }

    private static QuickFilterBarViewModel CreateSut(TestQuickFilterService service)
    {
        var repositoryFilteringManager = A.Fake<IRepositoryFilteringManager>();
        A.CallTo(() => repositoryFilteringManager.SelectedQueryParserKey).Returns("Default");
        return new QuickFilterBarViewModel(
            service,
            repositoryFilteringManager,
            [new TestNamedQueryParser("Default", text => new FreeText(text))],
            NullLogger.Instance);
    }

    private sealed class TestQuickFilterService : IQuickFilterService
    {
        public TestQuickFilterService(IEnumerable<QuickFilterModel>? initialFilters = null)
        {
            if (initialFilters != null)
            {
                Filters.AddRange(initialFilters);
            }
        }

        public List<QuickFilterModel> Filters { get; } = [];

        public List<(string label, IQuery query)> AddCalls { get; } = [];

        public List<(Guid id, bool isActive)> SetActiveCalls { get; } = [];

        public event EventHandler? Changed;

        public IReadOnlyList<QuickFilterModel> GetAll() => Filters.ToArray();

        public QuickFilterModel Add(string label, IQuery query)
        {
            AddCalls.Add((label, query));
            var model = new QuickFilterModel
            {
                Id = Guid.NewGuid(),
                Label = label,
                Query = query,
                IsActive = true,
            };
            Filters.Add(model);
            return model;
        }

        public void Remove(Guid id)
        {
            Filters.RemoveAll(x => x.Id == id);
        }

        public void SetActive(Guid id, bool isActive)
        {
            SetActiveCalls.Add((id, isActive));
            var filter = Filters.Find(x => x.Id == id);
            if (filter != null)
            {
                filter.IsActive = isActive;
            }
        }

        public void SetInverse(Guid id, bool isInverse)
        {
            var filter = Filters.Find(x => x.Id == id);
            if (filter != null)
            {
                filter.IsInverse = isInverse;
            }
        }

        public void UpdateLabel(Guid id, string newLabel)
        {
            var filter = Filters.Find(x => x.Id == id);
            if (filter != null)
            {
                filter.Label = newLabel;
            }
        }

        public void UpdateToolTip(Guid id, string newToolTip)
        {
            var filter = Filters.Find(x => x.Id == id);
            if (filter != null)
            {
                filter.ToolTip = newToolTip;
            }
        }

        public void UpdateOrder(Guid id, int newOrder)
        {
            var filter = Filters.Find(x => x.Id == id);
            if (filter != null)
            {
                filter.Order = newOrder;
            }
        }

        public QuickFilterModel? FindByQuery(IQuery query)
        {
            return Filters.Find(x => string.Equals(x.Query.ToString(), query.ToString(), StringComparison.OrdinalIgnoreCase));
        }

        public void RaiseChanged()
        {
            Changed?.Invoke(this, EventArgs.Empty);
        }
    }

    private sealed class TestNamedQueryParser : INamedQueryParser
    {
        private readonly Func<string, IQuery> _parse;

        public TestNamedQueryParser(string name, Func<string, IQuery> parse)
        {
            Name = name;
            _parse = parse;
        }

        public string Name { get; }

        public IQuery Parse(string text) => _parse(text);
    }
}