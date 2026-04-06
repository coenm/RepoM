namespace RepoM.App.Tests.ViewModels;

using System;
using System.Collections.Generic;
using AwesomeAssertions;
using FakeItEasy;
using RepoM.Api.QuickFilter;
using RepoM.App.ViewModels;
using RepoM.Core.Plugin.RepositoryFiltering.Clause.Terms;
using Xunit;

public class QuickFilterViewModelTests
{
    private static readonly Guid TestId = Guid.NewGuid();

    [Fact]
    public void Ctor_ShouldThrow_WhenArgumentNull()
    {
        // arrange
        var model = CreateModel();
        var service = A.Fake<IQuickFilterService>();

        // act
        Action act1 = () => _ = new QuickFilterViewModel(null!, service);
        Action act2 = () => _ = new QuickFilterViewModel(model, null!);

        // assert
        act1.Should().ThrowExactly<ArgumentNullException>();
        act2.Should().ThrowExactly<ArgumentNullException>();
    }

    [Fact]
    public void Properties_ShouldReflectModel()
    {
        // arrange & act
        var model = CreateModel(label: "WorkTag", toolTip: "My tooltip", isBuiltIn: false, order: 5);
        var sut = CreateSut(model);

        // assert
        sut.Id.Should().Be(model.Id);
        sut.Label.Should().Be("WorkTag");
        sut.IsBuiltIn.Should().BeFalse();
        sut.Order.Should().Be(5);
        sut.RawToolTip.Should().Be("My tooltip");
        sut.ToolTip.Should().Be("My tooltip");
        sut.HasToolTip.Should().BeTrue();
    }

    [Fact]
    public void DisplayLabel_ShouldTruncateLongLabels()
    {
        // arrange
        var model = CreateModel(label: "VeryLongLabelText");
        var sut = CreateSut(model);

        // act & assert
        sut.DisplayLabel.Should().Be("VeryLongLa...");
    }

    [Fact]
    public void DisplayLabel_ShouldNotTruncateShortLabels()
    {
        // arrange
        var model = CreateModel(label: "Short");
        var sut = CreateSut(model);

        // act & assert
        sut.DisplayLabel.Should().Be("Short");
    }

    [Fact]
    public void DisplayLabel_ShouldNotTruncateExactly10CharLabels()
    {
        // arrange
        var model = CreateModel(label: "1234567890");
        var sut = CreateSut(model);

        // act & assert
        sut.DisplayLabel.Should().Be("1234567890");
    }

    [Fact]
    public void ToolTip_ShouldReturnFavorites_ForBuiltInStarFilter()
    {
        // arrange
        var model = CreateModel(label: "\u2605", isBuiltIn: true);
        var sut = CreateSut(model);

        // act & assert
        sut.ToolTip.Should().Be("Favorites");
    }

    [Fact]
    public void ToolTip_ShouldReturnActive_ForBuiltInNonStarFilter()
    {
        // arrange
        var model = CreateModel(label: "\uD83D\uDC41", isBuiltIn: true);
        var sut = CreateSut(model);

        // act & assert
        sut.ToolTip.Should().Be("Active");
    }

    [Fact]
    public void ToolTip_ShouldFallBackToLabel_WhenToolTipIsEmpty()
    {
        // arrange
        var model = CreateModel(label: "Work", toolTip: "");
        var sut = CreateSut(model);

        // act & assert
        sut.ToolTip.Should().Be("Work");
    }

    [Fact]
    public void RawToolTip_ShouldReturnEmpty_ForBuiltIn()
    {
        // arrange
        var model = CreateModel(isBuiltIn: true);
        var sut = CreateSut(model);

        // act & assert
        sut.RawToolTip.Should().BeEmpty();
    }

    [Fact]
    public void IsActive_Set_ShouldCallService_AndRaisePropertyChanged()
    {
        // arrange
        var service = A.Fake<IQuickFilterService>();
        var model = CreateModel(isActive: false);
        var sut = CreateSut(model, service);
        var propertyNames = new List<string?>();
        sut.PropertyChanged += (_, e) => propertyNames.Add(e.PropertyName);

        // act
        sut.IsActive = true;

        // assert
        A.CallTo(() => service.SetActive(model.Id, true)).MustHaveHappenedOnceExactly();
        propertyNames.Should().Contain(nameof(QuickFilterViewModel.IsActive));
    }

    [Fact]
    public void IsActive_SetSameValue_ShouldNotCallService()
    {
        // arrange
        var service = A.Fake<IQuickFilterService>();
        var model = CreateModel(isActive: true);
        var sut = CreateSut(model, service);
        var propertyChangedCount = 0;
        sut.PropertyChanged += (_, _) => propertyChangedCount++;

        // act
        sut.IsActive = true;

        // assert
        A.CallTo(() => service.SetActive(A<Guid>._, A<bool>._)).MustNotHaveHappened();
        propertyChangedCount.Should().Be(0);
    }

    [Fact]
    public void IsInverse_Set_ShouldCallService_AndRaisePropertyChanged()
    {
        // arrange
        var service = A.Fake<IQuickFilterService>();
        var model = CreateModel(isInverse: false);
        var sut = CreateSut(model, service);
        var propertyNames = new List<string?>();
        sut.PropertyChanged += (_, e) => propertyNames.Add(e.PropertyName);

        // act
        sut.IsInverse = true;

        // assert
        A.CallTo(() => service.SetInverse(model.Id, true)).MustHaveHappenedOnceExactly();
        propertyNames.Should().Contain(nameof(QuickFilterViewModel.IsInverse));
    }

    [Fact]
    public void IsInverse_SetSameValue_ShouldNotCallService()
    {
        // arrange
        var service = A.Fake<IQuickFilterService>();
        var model = CreateModel(isInverse: false);
        var sut = CreateSut(model, service);

        // act
        sut.IsInverse = false;

        // assert
        A.CallTo(() => service.SetInverse(A<Guid>._, A<bool>._)).MustNotHaveHappened();
    }

    [Fact]
    public void Order_Set_ShouldCallService_AndRaisePropertyChanged()
    {
        // arrange
        var service = A.Fake<IQuickFilterService>();
        var model = CreateModel(order: 0);
        var sut = CreateSut(model, service);
        var propertyNames = new List<string?>();
        sut.PropertyChanged += (_, e) => propertyNames.Add(e.PropertyName);

        // act
        sut.Order = 5;

        // assert
        A.CallTo(() => service.UpdateOrder(model.Id, 5)).MustHaveHappenedOnceExactly();
        propertyNames.Should().Contain(nameof(QuickFilterViewModel.Order));
    }

    [Fact]
    public void Order_SetSameValue_ShouldNotCallService()
    {
        // arrange
        var service = A.Fake<IQuickFilterService>();
        var model = CreateModel(order: 3);
        var sut = CreateSut(model, service);

        // act
        sut.Order = 3;

        // assert
        A.CallTo(() => service.UpdateOrder(A<Guid>._, A<int>._)).MustNotHaveHappened();
    }

    [Fact]
    public void Toggle_FromOff_ShouldActivate()
    {
        // arrange
        var service = A.Fake<IQuickFilterService>();
        var model = CreateModel(isActive: false, isInverse: false);
        var sut = CreateSut(model, service);

        // act
        sut.Toggle();

        // assert
        A.CallTo(() => service.SetActive(model.Id, true)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public void Toggle_FromActiveNotInverse_ShouldSetInverse()
    {
        // arrange
        var service = A.Fake<IQuickFilterService>();
        var model = CreateModel(isActive: true, isInverse: false);
        var sut = CreateSut(model, service);

        // act
        sut.Toggle();

        // assert
        A.CallTo(() => service.SetInverse(model.Id, true)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public void Toggle_FromActiveInverse_ShouldDeactivate()
    {
        // arrange
        var service = A.Fake<IQuickFilterService>();
        var model = CreateModel(isActive: true, isInverse: true);
        var sut = CreateSut(model, service);

        // act
        sut.Toggle();

        // assert
        A.CallTo(() => service.SetInverse(model.Id, false)).MustHaveHappenedOnceExactly();
        A.CallTo(() => service.SetActive(model.Id, false)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public void UpdateLabel_ShouldCallService_AndRaisePropertyChanged()
    {
        // arrange
        var service = A.Fake<IQuickFilterService>();
        var model = CreateModel(label: "Old");
        var sut = CreateSut(model, service);
        var propertyNames = new List<string?>();
        sut.PropertyChanged += (_, e) => propertyNames.Add(e.PropertyName);

        // act
        sut.UpdateLabel("New");

        // assert
        A.CallTo(() => service.UpdateLabel(model.Id, "New")).MustHaveHappenedOnceExactly();
        propertyNames.Should().Contain(nameof(QuickFilterViewModel.Label));
        propertyNames.Should().Contain(nameof(QuickFilterViewModel.DisplayLabel));
        propertyNames.Should().Contain(nameof(QuickFilterViewModel.ToolTip));
        propertyNames.Should().Contain(nameof(QuickFilterViewModel.HasToolTip));
    }

    [Fact]
    public void UpdateToolTip_ShouldCallService_AndRaisePropertyChanged()
    {
        // arrange
        var service = A.Fake<IQuickFilterService>();
        var model = CreateModel();
        var sut = CreateSut(model, service);
        var propertyNames = new List<string?>();
        sut.PropertyChanged += (_, e) => propertyNames.Add(e.PropertyName);

        // act
        sut.UpdateToolTip("New tooltip");

        // assert
        A.CallTo(() => service.UpdateToolTip(model.Id, "New tooltip")).MustHaveHappenedOnceExactly();
        propertyNames.Should().Contain(nameof(QuickFilterViewModel.ToolTip));
        propertyNames.Should().Contain(nameof(QuickFilterViewModel.RawToolTip));
        propertyNames.Should().Contain(nameof(QuickFilterViewModel.HasToolTip));
    }

    [Fact]
    public void ToggleCommand_ShouldInvokeToggle()
    {
        // arrange
        var service = A.Fake<IQuickFilterService>();
        var model = CreateModel(isActive: false);
        var sut = CreateSut(model, service);

        // act
        sut.ToggleCommand.Execute(null);

        // assert
        A.CallTo(() => service.SetActive(model.Id, true)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public void RemoveCommand_ShouldCallServiceRemove()
    {
        // arrange
        var service = A.Fake<IQuickFilterService>();
        var model = CreateModel();
        var sut = CreateSut(model, service);

        // act
        sut.RemoveCommand.Execute(null);

        // assert
        A.CallTo(() => service.Remove(model.Id)).MustHaveHappenedOnceExactly();
    }

    private static QuickFilterViewModel CreateSut(QuickFilterModel? model = null, IQuickFilterService? service = null)
    {
        return new QuickFilterViewModel(
            model ?? CreateModel(),
            service ?? A.Fake<IQuickFilterService>());
    }

    private static QuickFilterModel CreateModel(
        string label = "Test",
        string toolTip = "",
        bool isBuiltIn = false,
        bool isActive = false,
        bool isInverse = false,
        int order = 0)
    {
        return new QuickFilterModel
        {
            Id = TestId,
            Label = label,
            Query = new SimpleTerm("tag", "test"),
            IsActive = isActive,
            IsInverse = isInverse,
            IsBuiltIn = isBuiltIn,
            ToolTip = toolTip,
            Order = order,
        };
    }
}
