namespace UiTests.VisualStudioCode;

using System;
using System.Text.RegularExpressions;
using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using Xunit.Abstractions;

public partial class VisualStudioPositionElement : AutomationElement
{
    private readonly ITestOutputHelper? _outputHelper;

    public VisualStudioPositionElement(FrameworkAutomationElementBase frameworkAutomationElement) : base(frameworkAutomationElement)
    {
    }

    public VisualStudioPositionElement(AutomationElement automationElement, ITestOutputHelper outputHelper)
        : base(automationElement)
    {
        _outputHelper = outputHelper;
    }

    public Position Position
    {
        get
        {
            var text = Name.Trim();

            if (string.IsNullOrWhiteSpace(text))
            {
                throw new NotSupportedException("Could not find line and column from empty text");
            }

            Match result = RegexLineCol().Match(text);
            if (result.Success)
            {
                return new Position(
                    int.Parse(result.Groups[1].Value),
                    int.Parse(result.Groups[2].Value));
            }

            throw new NotSupportedException($"Could not find line and column from text '{text}'");
        }
    }

    // TODO: also other type Ln 3, Col 4 (3 selected) stuf.
    [GeneratedRegex("Ln\\s([0-9]+),\\sCol\\s([0-9]+)", RegexOptions.Compiled | RegexOptions.Singleline)]
    private static partial Regex RegexLineCol();
}