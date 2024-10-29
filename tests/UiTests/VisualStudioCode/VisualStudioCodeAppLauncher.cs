namespace UiTests.VisualStudioCode;

using System;
using FlaUI.Core;
using FlaUI.UIA3;
using Xunit.Abstractions;

public class VisualStudioCodeAppLauncher
{
    private readonly string _exe;
    private readonly UIA3Automation _automation;
    private readonly ITestOutputHelper _outputHelper;

    public VisualStudioCodeAppLauncher(string exe, UIA3Automation automation, ITestOutputHelper outputHelper)
    {
        _exe = exe ?? throw new ArgumentNullException(nameof(exe));
        _automation = automation ?? throw new ArgumentNullException(nameof(automation));
        _outputHelper = outputHelper ?? throw new ArgumentNullException(nameof(outputHelper));
    }

    public VisualStudioCodeApp Launch()
    {
        var app = Application.Launch(
            @"""" + _exe + @"""",
            "--new-window  --disable-extensions");
        return new VisualStudioCodeApp(app, _automation, _outputHelper);
    }
}