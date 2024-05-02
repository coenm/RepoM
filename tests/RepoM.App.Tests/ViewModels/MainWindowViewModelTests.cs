namespace RepoM.App.Tests.ViewModels;

using FluentAssertions;
using RepoM.Api.Common;
using RepoM.App.Plugins;
using System;
using FakeItEasy;
using RepoM.App.RepositoryFiltering;
using RepoM.App.ViewModels;
using Xunit;
using RepoM.App.RepositoryOrdering;

public class MainWindowViewModelTests
{
    private readonly IAppSettingsService _appSettingsService;
    private readonly OrderingsViewModel _orderingsViewModel;
    private readonly QueryParsersViewModel _queryParsersViewModel;
    private readonly FiltersViewModel _filtersViewModel;
    private readonly PluginCollectionViewModel _pluginsViewModel;
    private readonly HelpViewModel _helpViewModel;
    
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
}