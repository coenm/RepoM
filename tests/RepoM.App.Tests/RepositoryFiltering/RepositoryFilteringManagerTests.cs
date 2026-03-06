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
using RepoM.Core.Repositories.Pinning;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

public class RepositoryFilteringManagerTests
{
    private readonly IAppSettingsService _appSettings = A.Fake<IAppSettingsService>();
    private readonly IFilterSettingsService _filterSettings = A.Fake<IFilterSettingsService>();
    private readonly IRepositoryMatcher _matcher = A.Fake<IRepositoryMatcher>();
    private readonly INamedQueryParser _queryParser = A.Fake<INamedQueryParser>();
    private RepositoryFilteringManager CreateSut()
    {
        A.CallTo(() => _queryParser.Name).Returns("Default");
        A.CallTo(() => _filterSettings.Configuration).Returns(new Dictionary<string, RepositoryFilterConfiguration>());

        return new RepositoryFilteringManager(
            _appSettings,
            _filterSettings,
            [_queryParser,],
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
        return new RepositoryViewModel(info, A.Fake<IPinningService>());
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
        predicates.Count.Should().BeGreaterThanOrEqualTo(2);
        predicates[^1](CreateVm()).Should().BeTrue();
    }
}
