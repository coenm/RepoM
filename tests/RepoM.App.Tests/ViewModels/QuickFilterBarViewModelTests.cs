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