namespace UiTests.VisualStudioCode;

using System;
using System.Threading.Tasks;
using System.Windows.Automation;
using FlaUI.Core;
using FlaUI.UIA3;
using FluentAssertions;
using Xunit.Abstractions;

public class VisualStudioCodeApp : IDisposable
{
    private readonly Application _app;
    private readonly UIA3Automation _automation;
    private readonly ITestOutputHelper _outputHelper;
    private VisualStudioCodeWindow? _window;

    public VisualStudioCodeApp(Application app, UIA3Automation automation, ITestOutputHelper outputHelper)
    {
        _app = app ?? throw new ArgumentNullException(nameof(app));
        _automation = automation ?? throw new ArgumentNullException(nameof(automation));
        _outputHelper = outputHelper ?? throw new ArgumentNullException(nameof(outputHelper));
    }

    public VisualStudioCodeWindow Window => _window ??= WaitForWindow();

    public VisualStudioCodeWindow WaitForWindow()
    {
        if (_window != null)
        {
            return _window;
        }

        _app.WaitWhileMainHandleIsMissing(TimeSpan.FromSeconds(20));
        _app.WaitWhileBusy(TimeSpan.FromSeconds(20));
        return new VisualStudioCodeWindow(_app.GetMainWindow(_automation).FrameworkAutomationElement, _outputHelper);
    }
    
    public void Dispose()
    {
        _app.Dispose();
    }
}