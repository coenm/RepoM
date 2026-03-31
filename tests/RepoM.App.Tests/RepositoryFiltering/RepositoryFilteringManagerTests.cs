namespace RepoM.App.Tests.RepositoryFiltering;

using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using FakeItEasy;
using AwesomeAssertions;
using RepoM.Api.Common;
using RepoM.Api.Git;
using RepoM.App.RepositoryFiltering;
using RepoM.Core.Plugin.Repository;
using RepoM.Core.Plugin.RepositoryFiltering;
using RepoM.Core.Plugin.RepositoryFiltering.Clause;
using RepoM.Core.Plugin.RepositoryFiltering.Configuration;
using RepoM.Core.Repositories.Model;
using RepoM.Core.Repositories.Monitoring;
using RepoM.Core.Repositories.Favorite;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

public class RepositoryFilteringManagerTests
{
    private readonly IAppSettingsService _appSettings = A.Fake<IAppSettingsService>();
    private readonly IFilterSettingsService _filterSettings = A.Fake<IFilterSettingsService>();
    private readonly IRepositoryMatcher _matcher = A.Fake<IRepositoryMatcher>();
    private readonly INamedQueryParser _queryParser = A.Fake<INamedQueryParser>();

    private RepositoryFilteringManager CreateSut(
        INamedQueryParser[]? parsers = null,
        Dictionary<string, RepositoryFilterConfiguration>? filterConfig = null)
    {
        A.CallTo(() => _queryParser.Name).Returns("Default");
        A.CallTo(() => _filterSettings.Configuration)
            .Returns(filterConfig ?? new Dictionary<string, RepositoryFilterConfiguration>());

        return new RepositoryFilteringManager(
            _appSettings,
            _filterSettings,
            parsers ?? [_queryParser,],
            _matcher,
            NullLogger.Instance);
    }

    private static RepositoryViewModel CreateVm(string name = "test", string path = @"c:\repos\test")
    {
        var info = new RepositoryInfo
        {
            Path = path,
            SafePath = path.Replace('\\', '/'),
            Name = name,
        };
        return new RepositoryViewModel(info, A.Fake<IFavoriteService>(), A.Fake<IRepositoryMonitoringService>(), A.Fake<IRepositoryMonitoringEvents>());
    }

    [Fact]
    public void CreateFilterObservable_ShouldThrow_WhenTextInputIsNull()
    {
        // arrange
        RepositoryFilteringManager sut = CreateSut();

        // act
        Action act = () => sut.CreateFilterObservable(null!);

        // assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void CreateFilterObservable_ShouldEmitPredicate_WhenTextInputEmits()
    {
        // arrange
        RepositoryFilteringManager sut = CreateSut();
        var textSubject = new BehaviorSubject<string>(string.Empty);
        IObservable<Func<RepositoryViewModel, bool>> observable = sut.CreateFilterObservable(textSubject);
        Func<RepositoryViewModel, bool>? lastPredicate = null;

        // act
        using IDisposable sub = observable.Subscribe(p => lastPredicate = p);

        // assert
        lastPredicate.Should().NotBeNull();
    }

    [Fact]
    public void CreateFilterObservable_ShouldMatchAll_WhenQueryIsEmpty()
    {
        // arrange
        RepositoryFilteringManager sut = CreateSut();
        A.CallTo(() => _matcher.Matches(A<IRepository>._, A<IQuery>._)).Returns(true);
        var textSubject = new BehaviorSubject<string>(string.Empty);
        Func<RepositoryViewModel, bool>? lastPredicate = null;
        using IDisposable sub = sut.CreateFilterObservable(textSubject).Subscribe(p => lastPredicate = p);

        // act
        bool result = lastPredicate!(CreateVm());

        // assert
        result.Should().BeTrue();
    }

    [Fact]
    public void CreateFilterObservable_ShouldUseMatcher_WhenQueryIsNotEmpty()
    {
        // arrange
        RepositoryFilteringManager sut = CreateSut();
        var parsedQuery = A.Fake<IQuery>();
        A.CallTo(() => _queryParser.Parse("search")).Returns(parsedQuery);
        A.CallTo(() => _matcher.Matches(A<IRepository>._, A<IQuery>._)).Returns(true);
        A.CallTo(() => _matcher.Matches(A<IRepository>._, parsedQuery)).Returns(false);

        var textSubject = new BehaviorSubject<string>("search");
        Func<RepositoryViewModel, bool>? lastPredicate = null;
        using IDisposable sub = sut.CreateFilterObservable(textSubject).Subscribe(p => lastPredicate = p);

        // act
        bool result = lastPredicate!(CreateVm());

        // assert
        result.Should().BeFalse();
        A.CallTo(() => _matcher.Matches(A<IRepository>._, parsedQuery)).MustHaveHappened();
    }

    [Fact]
    public void CreateFilterObservable_ShouldEmitNewPredicate_WhenFilterChanges()
    {
        // arrange
        RepositoryFilteringManager sut = CreateSut();
        A.CallTo(() => _matcher.Matches(A<IRepository>._, A<IQuery>._)).Returns(true);
        var textSubject = new BehaviorSubject<string>(string.Empty);
        var predicates = new List<Func<RepositoryViewModel, bool>>();
        using IDisposable sub = sut.CreateFilterObservable(textSubject).Subscribe(p => predicates.Add(p));
        var countBefore = predicates.Count;

        // act — changing the filter triggers a new predicate emission
        sut.SetFilter("Default");

        // assert
        predicates.Count.Should().BeGreaterThan(countBefore);
    }

    [Fact]
    public void CreateFilterObservable_ShouldEmitNewPredicate_WhenQueryParserChanges()
    {
        // arrange
        RepositoryFilteringManager sut = CreateSut();
        A.CallTo(() => _matcher.Matches(A<IRepository>._, A<IQuery>._)).Returns(true);
        var textSubject = new BehaviorSubject<string>(string.Empty);
        var predicates = new List<Func<RepositoryViewModel, bool>>();
        using IDisposable sub = sut.CreateFilterObservable(textSubject).Subscribe(p => predicates.Add(p));
        var countBefore = predicates.Count;

        // act
        sut.SetQueryParser("Default");

        // assert
        predicates.Count.Should().BeGreaterThan(countBefore);
    }

    [Fact]
    public void CreateFilterObservable_ShouldEmitNewPredicate_WhenTextChanges()
    {
        // arrange
        RepositoryFilteringManager sut = CreateSut();
        A.CallTo(() => _matcher.Matches(A<IRepository>._, A<IQuery>._)).Returns(true);
        var textSubject = new BehaviorSubject<string>(string.Empty);
        var predicates = new List<Func<RepositoryViewModel, bool>>();
        using IDisposable sub = sut.CreateFilterObservable(textSubject).Subscribe(p => predicates.Add(p));
        var countBefore = predicates.Count;

        // act
        textSubject.OnNext("new search");

        // assert
        predicates.Count.Should().BeGreaterThan(countBefore);
    }

    [Fact]
    public void CreateFilterObservable_PredicateShouldReturnFalse_WhenMatcherThrows()
    {
        // arrange
        RepositoryFilteringManager sut = CreateSut();
        A.CallTo(() => _matcher.Matches(A<IRepository>._, A<IQuery>._)).Throws<InvalidOperationException>();

        var textSubject = new BehaviorSubject<string>(string.Empty);
        Func<RepositoryViewModel, bool>? lastPredicate = null;
        using IDisposable sub = sut.CreateFilterObservable(textSubject).Subscribe(p => lastPredicate = p);

        // act
        bool result = lastPredicate!(CreateVm());

        // assert
        result.Should().BeFalse();
    }

    [Fact]
    public void CreateFilterObservable_PredicateShouldReturnFalse_WhenQueryParserThrows()
    {
        // arrange
        RepositoryFilteringManager sut = CreateSut();
        A.CallTo(() => _queryParser.Parse(A<string>._)).Throws<Exception>();

        var textSubject = new BehaviorSubject<string>("RepoM OR");
        Func<RepositoryViewModel, bool>? lastPredicate = null;
        using IDisposable sub = sut.CreateFilterObservable(textSubject).Subscribe(p => lastPredicate = p);

        // act
        bool result = lastPredicate!(CreateVm());

        // assert — invalid query should hide everything, not crash the pipeline
        result.Should().BeFalse();
    }

    [Fact]
    public void CreateFilterObservable_ShouldContinueEmitting_AfterQueryParserThrows()
    {
        // arrange
        RepositoryFilteringManager sut = CreateSut();
        A.CallTo(() => _queryParser.Parse("invalid OR")).Throws<Exception>();
        A.CallTo(() => _queryParser.Parse("valid")).Returns(A.Fake<IQuery>());
        A.CallTo(() => _matcher.Matches(A<IRepository>._, A<IQuery>._)).Returns(true);

        var textSubject = new BehaviorSubject<string>("invalid OR");
        var predicates = new List<Func<RepositoryViewModel, bool>>();
        using IDisposable sub = sut.CreateFilterObservable(textSubject).Subscribe(p => predicates.Add(p));

        // act — fix the query
        textSubject.OnNext("valid");

        // assert — pipeline should still be alive and emitting new predicates
        predicates.Should().HaveCountGreaterThanOrEqualTo(2);
        predicates[^1](CreateVm()).Should().BeTrue();
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenAppSettingsServiceIsNull()
    {
        A.CallTo(() => _queryParser.Name).Returns("Default");
        A.CallTo(() => _filterSettings.Configuration).Returns(new Dictionary<string, RepositoryFilterConfiguration>());

        Action act = () => new RepositoryFilteringManager(null!, _filterSettings, [_queryParser,], _matcher, NullLogger.Instance);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenFilterSettingsServiceIsNull()
    {
        A.CallTo(() => _queryParser.Name).Returns("Default");

        Action act = () => new RepositoryFilteringManager(_appSettings, null!, [_queryParser,], _matcher, NullLogger.Instance);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenQueryParsersIsNull()
    {
        A.CallTo(() => _filterSettings.Configuration).Returns(new Dictionary<string, RepositoryFilterConfiguration>());

        Action act = () => new RepositoryFilteringManager(_appSettings, _filterSettings, null!, _matcher, NullLogger.Instance);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenMatcherIsNull()
    {
        A.CallTo(() => _queryParser.Name).Returns("Default");
        A.CallTo(() => _filterSettings.Configuration).Returns(new Dictionary<string, RepositoryFilterConfiguration>());

        Action act = () => new RepositoryFilteringManager(_appSettings, _filterSettings, [_queryParser,], null!, NullLogger.Instance);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenLoggerIsNull()
    {
        A.CallTo(() => _queryParser.Name).Returns("Default");
        A.CallTo(() => _filterSettings.Configuration).Returns(new Dictionary<string, RepositoryFilterConfiguration>());

        Action act = () => new RepositoryFilteringManager(_appSettings, _filterSettings, [_queryParser,], _matcher, null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenQueryParsersIsEmpty()
    {
        A.CallTo(() => _filterSettings.Configuration).Returns(new Dictionary<string, RepositoryFilterConfiguration>());

        Action act = () => new RepositoryFilteringManager(_appSettings, _filterSettings, [], _matcher, NullLogger.Instance);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Constructor_ShouldUseLuceneParser_WhenAvailable()
    {
        // arrange
        var luceneParser = A.Fake<INamedQueryParser>();
        A.CallTo(() => luceneParser.Name).Returns("Lucene");

        var filterConfig = new Dictionary<string, RepositoryFilterConfiguration>
        {
            ["MyFilter"] = new RepositoryFilterConfiguration
            {
                Name = "MyFilter",
                Description = "Test filter",
                Filter = new QueryConfiguration { Kind = "query@1", Query = "is:pinned", },
                AlwaysVisible = new QueryConfiguration { Kind = "query@1", Query = "is:pinned", },
            },
        };

        // act
        RepositoryFilteringManager sut = CreateSut([_queryParser, luceneParser,], filterConfig);

        // assert — Lucene parser should have been used for "query@1" kind
        A.CallTo(() => luceneParser.Parse("is:pinned")).MustHaveHappened();
    }

    [Fact]
    public void Constructor_ShouldUseDefaultParser_WhenFilterKindIsNotQuery1()
    {
        // arrange
        var filterConfig = new Dictionary<string, RepositoryFilterConfiguration>
        {
            ["MyFilter"] = new RepositoryFilterConfiguration
            {
                Name = "MyFilter",
                Description = "Test filter",
                Filter = new QueryConfiguration { Kind = "other", Query = "some query", },
                AlwaysVisible = new QueryConfiguration { Kind = string.Empty, Query = string.Empty, },
            },
        };

        // act
        RepositoryFilteringManager sut = CreateSut(filterConfig: filterConfig);

        // assert — Default parser should have been used
        A.CallTo(() => _queryParser.Parse("some query")).MustHaveHappened();
    }

    [Fact]
    public void Constructor_ShouldNotAddDefaultFilter_WhenAlreadyPresent()
    {
        // arrange
        var filterConfig = new Dictionary<string, RepositoryFilterConfiguration>
        {
            ["Default"] = new RepositoryFilterConfiguration
            {
                Name = "Default",
                Description = "Custom default",
                Filter = new QueryConfiguration { Kind = string.Empty, Query = string.Empty, },
                AlwaysVisible = new QueryConfiguration { Kind = string.Empty, Query = string.Empty, },
            },
        };

        // act
        RepositoryFilteringManager sut = CreateSut(filterConfig: filterConfig);

        // assert — should only have the single "Default" entry
        sut.FilterKeys.Should().ContainSingle(k => k == "Default");
    }

    [Fact]
    public void Constructor_ShouldRestoreSavedQueryParserKey()
    {
        // arrange
        A.CallTo(() => _appSettings.QueryParserKey).Returns("Default");

        // act
        RepositoryFilteringManager sut = CreateSut();

        // assert
        sut.SelectedQueryParserKey.Should().Be("Default");
    }

    [Fact]
    public void Constructor_ShouldFallbackToFirst_WhenSavedQueryParserKeyIsInvalid()
    {
        // arrange
        A.CallTo(() => _appSettings.QueryParserKey).Returns("NonExistent");

        // act
        RepositoryFilteringManager sut = CreateSut();

        // assert — should fall back to first available parser
        sut.SelectedQueryParserKey.Should().Be("Default");
    }

    [Fact]
    public void Constructor_ShouldRestoreSavedSelectedFilter()
    {
        // arrange
        A.CallTo(() => _appSettings.SelectedFilter).Returns("Default");

        // act
        RepositoryFilteringManager sut = CreateSut();

        // assert
        sut.SelectedFilterKey.Should().Be("Default");
    }

    [Fact]
    public void Constructor_ShouldFallbackToFirst_WhenSavedFilterKeyIsInvalid()
    {
        // arrange
        A.CallTo(() => _appSettings.SelectedFilter).Returns("NonExistent");

        // act
        RepositoryFilteringManager sut = CreateSut();

        // assert — should fall back to first filter
        sut.SelectedFilterKey.Should().Be("Default");
    }

    [Fact]
    public void SetQueryParser_ShouldReturnFalse_WhenKeyIsUnknown()
    {
        // arrange
        RepositoryFilteringManager sut = CreateSut();

        // act
        bool result = sut.SetQueryParser("UnknownParser");

        // assert
        result.Should().BeFalse();
    }

    [Fact]
    public void SetQueryParser_ShouldReturnTrue_WhenKeyIsValid()
    {
        // arrange
        RepositoryFilteringManager sut = CreateSut();

        // act
        bool result = sut.SetQueryParser("Default");

        // assert
        result.Should().BeTrue();
        sut.SelectedQueryParserKey.Should().Be("Default");
    }

    [Fact]
    public void SetQueryParser_ShouldPersistKeyToAppSettings()
    {
        // arrange
        RepositoryFilteringManager sut = CreateSut();

        // act
        sut.SetQueryParser("Default");

        // assert
        A.CallToSet(() => _appSettings.QueryParserKey).To("Default").MustHaveHappened();
    }

    [Fact]
    public void SetQueryParser_ShouldRaiseSelectedQueryParserChanged()
    {
        // arrange
        RepositoryFilteringManager sut = CreateSut();
        string? raisedKey = null;
        sut.SelectedQueryParserChanged += (_, key) => raisedKey = key;

        // act
        sut.SetQueryParser("Default");

        // assert
        raisedKey.Should().Be("Default");
    }

    [Fact]
    public void SetFilter_ShouldReturnFalse_WhenKeyIsUnknown()
    {
        // arrange
        RepositoryFilteringManager sut = CreateSut();

        // act
        bool result = sut.SetFilter("NonExistent");

        // assert
        result.Should().BeFalse();
    }

    [Fact]
    public void SetFilter_ShouldReturnTrue_WhenKeyIsValid()
    {
        // arrange
        RepositoryFilteringManager sut = CreateSut();

        // act
        bool result = sut.SetFilter("Default");

        // assert
        result.Should().BeTrue();
        sut.SelectedFilterKey.Should().Be("Default");
    }

    [Fact]
    public void SetFilter_ShouldPersistKeyToAppSettings()
    {
        // arrange
        RepositoryFilteringManager sut = CreateSut();

        // act
        sut.SetFilter("Default");

        // assert
        A.CallToSet(() => _appSettings.SelectedFilter).To("Default").MustHaveHappened();
    }

    [Fact]
    public void SetFilter_ShouldRaiseSelectedFilterChanged()
    {
        // arrange
        RepositoryFilteringManager sut = CreateSut();
        string? raisedKey = null;
        sut.SelectedFilterChanged += (_, key) => raisedKey = key;

        // act
        sut.SetFilter("Default");

        // assert
        raisedKey.Should().Be("Default");
    }

    [Fact]
    public void QueryParserKeys_ShouldReturnAllParserNames()
    {
        // arrange
        RepositoryFilteringManager sut = CreateSut();

        // assert
        sut.QueryParserKeys.Should().ContainSingle().Which.Should().Be("Default");
    }

    [Fact]
    public void FilterKeys_ShouldContainDefaultFilter()
    {
        // arrange
        RepositoryFilteringManager sut = CreateSut();

        // assert
        sut.FilterKeys.Should().Contain("Default");
    }

    [Fact]
    public void Predicate_ShouldReturnTrue_WhenAlwaysVisibleFilterMatches()
    {
        // arrange
        var alwaysVisibleQuery = A.Fake<IQuery>();
        A.CallTo(() => _queryParser.Parse("is:pinned")).Returns(alwaysVisibleQuery);
        A.CallTo(() => _matcher.Matches(A<IRepository>._, alwaysVisibleQuery)).Returns(true);
        // preFilter rejects, but alwaysVisible overrides
        var preFilterQuery = A.Fake<IQuery>();
        A.CallTo(() => _queryParser.Parse("is:special")).Returns(preFilterQuery);
        A.CallTo(() => _matcher.Matches(A<IRepository>._, preFilterQuery)).Returns(false);

        var filterConfig = new Dictionary<string, RepositoryFilterConfiguration>
        {
            ["Pinned"] = new RepositoryFilterConfiguration
            {
                Name = "Pinned",
                Description = "Pinned filter",
                AlwaysVisible = new QueryConfiguration { Kind = "query@1", Query = "is:pinned", },
                Filter = new QueryConfiguration { Kind = "query@1", Query = "is:special", },
            },
        };

        var luceneParser = A.Fake<INamedQueryParser>();
        A.CallTo(() => luceneParser.Name).Returns("Lucene");
        A.CallTo(() => luceneParser.Parse("is:pinned")).Returns(alwaysVisibleQuery);
        A.CallTo(() => luceneParser.Parse("is:special")).Returns(preFilterQuery);

        RepositoryFilteringManager sut = CreateSut([_queryParser, luceneParser,], filterConfig);
        sut.SetFilter("Pinned");

        var textSubject = new BehaviorSubject<string>(string.Empty);
        Func<RepositoryViewModel, bool>? lastPredicate = null;
        using IDisposable sub = sut.CreateFilterObservable(textSubject).Subscribe(p => lastPredicate = p);

        // act
        bool result = lastPredicate!(CreateVm());

        // assert — alwaysVisible match should return true
        result.Should().BeTrue();
    }

    [Fact]
    public void Predicate_ShouldReturnFalse_WhenPreFilterRejects()
    {
        // arrange
        var preFilterQuery = A.Fake<IQuery>();
        A.CallTo(() => _queryParser.Parse("is:special")).Returns(preFilterQuery);
        A.CallTo(() => _matcher.Matches(A<IRepository>._, preFilterQuery)).Returns(false);

        var filterConfig = new Dictionary<string, RepositoryFilterConfiguration>
        {
            ["Special"] = new RepositoryFilterConfiguration
            {
                Name = "Special",
                Description = "Special filter",
                AlwaysVisible = new QueryConfiguration { Kind = string.Empty, Query = string.Empty, },
                Filter = new QueryConfiguration { Kind = "other", Query = "is:special", },
            },
        };

        RepositoryFilteringManager sut = CreateSut(filterConfig: filterConfig);
        sut.SetFilter("Special");

        var textSubject = new BehaviorSubject<string>(string.Empty);
        Func<RepositoryViewModel, bool>? lastPredicate = null;
        using IDisposable sub = sut.CreateFilterObservable(textSubject).Subscribe(p => lastPredicate = p);

        // act
        bool result = lastPredicate!(CreateVm());

        // assert — preFilter rejection should return false
        result.Should().BeFalse();
    }
}
