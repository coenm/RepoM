namespace RepoM.App.Tests.RepositoryOrdering;

using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using FakeItEasy;
using AwesomeAssertions;
using RepoM.Api.Common;
using RepoM.Api.Git;
using RepoM.App.RepositoryOrdering;
using RepoM.Core.Plugin.RepositoryOrdering;
using RepoM.Core.Plugin.RepositoryOrdering.Configuration;
using RepoM.Core.Repositories.Model;
using RepoM.Core.Repositories.Pinning;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

public class RepositoryComparerManagerTests
{
    private readonly IAppSettingsService _appSettings = A.Fake<IAppSettingsService>();
    private readonly ICompareSettingsService _compareSettings = A.Fake<ICompareSettingsService>();
    private readonly IRepositoryComparerFactory _factory = A.Fake<IRepositoryComparerFactory>();

    private RepositoryComparerManager CreateSut()
    {
        A.CallTo(() => _compareSettings.Configuration)
            .Returns(new Dictionary<string, IRepositoriesComparerConfiguration>());

        return new RepositoryComparerManager(
            _appSettings,
            _compareSettings,
            _factory,
            NullLogger.Instance);
    }

    private static RepositoryViewModel CreateVm(string name)
    {
        var info = new RepositoryInfo
        {
            Path = @$"c:\repos\{name}",
            SafePath = $"/repos/{name}",
            Name = name,
        };
        return new RepositoryViewModel(info, A.Fake<IPinningService>());
    }

    [Fact]
    public void SortObservable_ShouldEmitComparerImmediately()
    {
        // arrange
        RepositoryComparerManager sut = CreateSut();
        IComparer<RepositoryViewModel>? emitted = null;

        // act
        using IDisposable sub = sut.SortObservable.Subscribe(c => emitted = c);

        // assert
        emitted.Should().NotBeNull();
    }

    [Fact]
    public void SortObservable_EmittedComparer_ShouldSortByName()
    {
        // arrange
        RepositoryComparerManager sut = CreateSut();
        IComparer<RepositoryViewModel>? comparer = null;
        using IDisposable sub = sut.SortObservable.Subscribe(c => comparer = c);

        RepositoryViewModel vmA = CreateVm("Alpha");
        RepositoryViewModel vmB = CreateVm("Beta");

        // act
        int result = comparer!.Compare(vmA, vmB);

        // assert — default is AzComparer (alphabetical)
        result.Should().BeNegative();
    }

    [Fact]
    public void SortObservable_ShouldEmitNewComparer_WhenComparerKeyChanges()
    {
        // arrange
        RepositoryComparerManager sut = CreateSut();
        var comparers = new List<IComparer<RepositoryViewModel>>();
        using IDisposable sub = sut.SortObservable.Subscribe(c => comparers.Add(c));
        var countBefore = comparers.Count;

        // act — SetRepositoryComparer with "Default" triggers the event
        sut.SetRepositoryComparer("Default");

        // assert
        comparers.Count.Should().BeGreaterThan(countBefore);
    }

    [Fact]
    public void SortObservable_ShouldNotEmit_WhenInvalidKeyIsSet()
    {
        // arrange
        RepositoryComparerManager sut = CreateSut();
        var comparers = new List<IComparer<RepositoryViewModel>>();
        using IDisposable sub = sut.SortObservable.Subscribe(c => comparers.Add(c));
        var countBefore = comparers.Count;

        // act
        bool result = sut.SetRepositoryComparer("nonexistent");

        // assert
        result.Should().BeFalse();
        comparers.Should().HaveCount(countBefore);
    }
}
