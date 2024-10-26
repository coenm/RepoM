namespace UiTests.VisualStudioCode;

using System;
using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;
using FluentAssertions;
using Xunit.Abstractions;

public class StatusBar : AutomationElement
{
    private readonly ITestOutputHelper _outputHelper;

    public StatusBar(FrameworkAutomationElementBase frameworkAutomationElement) : base(frameworkAutomationElement)
    {
        throw new NotImplementedException();
    }

    public StatusBar(AutomationElement automationElement, ITestOutputHelper outputHelper) : base(automationElement)
    {
        _outputHelper = outputHelper;
    }

    public Button NotificationButton
    {
        get
        {
            AutomationElement selection = this.FindFirstDescendant("status.notifications");
            selection.Should().NotBeNull();

            Button btn = selection.FindFirstChild(cf => cf.ByControlType(ControlType.Button)).AsButton();
            btn.Should().NotBeNull();

            return btn;
        }
    }

    public PositionElement PositionElement
    {
        get
        {
            var selection = FindFirstDescendant("status.editor.selection");
            selection.Should().NotBeNull();

            var children = selection.FindAllChildren();
            children.Should().HaveCount(1);

            return new PositionElement(children[0], _outputHelper);
        }
    }
}