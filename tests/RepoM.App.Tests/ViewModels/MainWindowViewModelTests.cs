namespace RepoM.App.Tests.ViewModels;

using AwesomeAssertions;
using RepoM.Api.Common;
using RepoM.App.Plugins;
using System;
using System.Collections.Generic;
using FakeItEasy;
using RepoM.App.RepositoryFiltering;
using RepoM.App.ViewModels;
using Xunit;
using RepoM.App.RepositoryOrdering;
using RepoM.Api.Git.AutoFetch;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Input;

public class MainWindowViewModelTests
{
    private readonly IAppSettingsService _appSettingsService;
    private readonly OrderingsViewModel _orderingsViewModel;
    private readonly QueryParsersViewModel _queryParsersViewModel;
    private readonly FiltersViewModel _filtersViewModel;
    private readonly PluginCollectionViewModel _pluginsViewModel;
    private readonly HelpViewModel _helpViewModel;
    private readonly ICommand _saveQuickFilterCommand;
    private readonly ICommand _addQuickFilterTagCommand;
    
    public MainWindowViewModelTests()
    {
        IRepositoryComparerManager repositoryComparerManager = A.Fake<IRepositoryComparerManager>();
        IThreadDispatcher threadDispatcher = A.Fake<IThreadDispatcher>();
        IRepositoryFilteringManager repositoryFilterManager = A.Fake<IRepositoryFilteringManager>();
        IModuleManager moduleManager = A.Fake<IModuleManager>();

        _appSettingsService = A.Fake<IAppSettingsService>();
        _orderingsViewModel = new OrderingsViewModel(repositoryComparerManager, threadDispatcher);
        _queryParsersViewModel = new QueryParsersViewModel(repositoryFilterManager, threadDispatcher);
        _filtersViewModel = new FiltersViewModel(repositoryFilterManager, threadDispatcher);
        _pluginsViewModel = new PluginCollectionViewModel(moduleManager);
        _helpViewModel = new HelpViewModel(A.Fake<ITranslationService>());
        _saveQuickFilterCommand = A.Dummy<ICommand>();
        _addQuickFilterTagCommand = A.Dummy<ICommand>();
    }

    [Fact]
    public void Ctor_ShouldThrown_WhenArgumentIsNull()
    {
        // arrange

        // act
        Action act1 = () => _ = new MainWindowViewModel(_appSettingsService, _orderingsViewModel, _queryParsersViewModel, _filtersViewModel, null!, _helpViewModel);
        Action act2 = () => _ = new MainWindowViewModel(_appSettingsService, _orderingsViewModel, _queryParsersViewModel, null!, _pluginsViewModel, _helpViewModel);
        Action act3 = () => _ = new MainWindowViewModel(_appSettingsService, _orderingsViewModel, null!, _filtersViewModel, _pluginsViewModel, _helpViewModel);
        Action act4 = () => _ = new MainWindowViewModel(_appSettingsService, null!, _queryParsersViewModel, _filtersViewModel, _pluginsViewModel, _helpViewModel);
        Action act5 = () => _ = new MainWindowViewModel(null!, _orderingsViewModel, _queryParsersViewModel, _filtersViewModel, _pluginsViewModel, _helpViewModel);
        Action act6 = () => _ = new MainWindowViewModel(_appSettingsService, _orderingsViewModel, _queryParsersViewModel, _filtersViewModel, _pluginsViewModel, null!);

        // assert
        act1.Should().ThrowExactly<ArgumentNullException>();
        act2.Should().ThrowExactly<ArgumentNullException>();
        act3.Should().ThrowExactly<ArgumentNullException>();
        act4.Should().ThrowExactly<ArgumentNullException>();
        act5.Should().ThrowExactly<ArgumentNullException>();
        act6.Should().ThrowExactly<ArgumentNullException>();
    }

    [Fact]
    public void Ctor_ShouldInitializeProperties()
    {
        // arrange

        // act
        var sut = new MainWindowViewModel(_appSettingsService, _orderingsViewModel, _queryParsersViewModel, _filtersViewModel, _pluginsViewModel, _helpViewModel);

        // assert
        sut.QueryParsers.Should().BeSameAs(_queryParsersViewModel);
        sut.Orderings.Should().BeSameAs(_orderingsViewModel);
        sut.Filters.Should().BeSameAs(_filtersViewModel);
        sut.Plugins.Should().BeSameAs(_pluginsViewModel);
    }

    [Fact]
    public void Ctor_WithCommands_ShouldInitializeCommandProperties()
    {
        // arrange

        // act
        var sut = new MainWindowViewModel(
            _appSettingsService,
            _orderingsViewModel,
            _queryParsersViewModel,
            _filtersViewModel,
            _pluginsViewModel,
            _helpViewModel,
            new MainWindowQuickFilterCommands(_saveQuickFilterCommand, _addQuickFilterTagCommand));

        // assert
        sut.SaveQuickFilterCommand.Should().BeSameAs(_saveQuickFilterCommand);
        sut.AddQuickFilterTagCommand.Should().BeSameAs(_addQuickFilterTagCommand);
        sut.Help.Should().BeSameAs(_helpViewModel);
    }

    [Fact]
    public void AutoFetchAdequate_ShouldSetUnderlyingAutoFetchMode_AndRaisePropertyChanged()
    {
        // arrange
        var propertyNames = new List<string?>();
        var appSettingsService = new TestAppSettingsService { AutoFetchMode = AutoFetchMode.Off };
        var sut = new MainWindowViewModel(appSettingsService, _orderingsViewModel, _queryParsersViewModel, _filtersViewModel, _pluginsViewModel, _helpViewModel);
        sut.PropertyChanged += (_, e) => propertyNames.Add(e.PropertyName);

        // act
        sut.AutoFetchAdequate = true;

        // assert
        appSettingsService.AutoFetchMode.Should().Be(AutoFetchMode.Adequate);
        sut.AutoFetchAdequate.Should().BeTrue();
        propertyNames.Should().Contain(nameof(sut.AutoFetchAdequate));
        propertyNames.Should().Contain(nameof(sut.AutoFetchOff));
    }

    [Fact]
    public void PruneOnFetch_ShouldReadAndWriteThroughAppSettingsService()
    {
        // arrange
        var appSettingsService = new TestAppSettingsService();
        var sut = new MainWindowViewModel(appSettingsService, _orderingsViewModel, _queryParsersViewModel, _filtersViewModel, _pluginsViewModel, _helpViewModel);

        // act
        sut.PruneOnFetch = true;

        // assert
        sut.PruneOnFetch.Should().BeTrue();
        appSettingsService.PruneOnFetch.Should().BeTrue();
    }

    private sealed class TestAppSettingsService : IAppSettingsService
    {
        public AutoFetchMode AutoFetchMode { get; set; }

        public bool PruneOnFetch { get; set; }

        public void UpdateMenuSize(string resolution, MenuSize size)
        {
        }

        public bool TryGetMenuSize(string resolution, [NotNullWhen(true)] out MenuSize? size)
        {
            size = null;
            return false;
        }

        public List<string> ReposRootDirectories { get; set; } = [];

        public int MenuPrefetchHoverDelayMilliseconds => 2000;

        public string SortKey { get; set; } = string.Empty;

        public string QueryParserKey { get; set; } = string.Empty;

        public string SelectedFilter { get; set; } = string.Empty;

        public List<PluginSettings> Plugins { get; set; } = [];

        public void RegisterInvalidationHandler(Action handler)
        {
        }
    }
}