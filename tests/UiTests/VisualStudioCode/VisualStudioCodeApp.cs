namespace UiTests.VisualStudioCode;

using System;
using System.Threading;
using System.Threading.Tasks;
using FlaUI.Core;
using FlaUI.UIA3;
using UiTests.Utils;
using UiTests.VisualStudioCode.WebSockets;
using UiTests.VisualStudioCode.WebSockets.Commands;
using Xunit.Abstractions;

public class VisualStudioCodeApp : IDisposable
{
    private readonly Application _app;
    private readonly UIA3Automation _automation;
    private readonly ITestOutputHelper _outputHelper;
    private VisualStudioCodeWindow? _window;
    private readonly VisualStudioWebSocketAutomation _vscWebSocketAutomation;

    public VisualStudioCodeApp(Application app, UIA3Automation automation, ITestOutputHelper outputHelper, Uri wsUri)
    {
        _app = app ?? throw new ArgumentNullException(nameof(app));
        _automation = automation ?? throw new ArgumentNullException(nameof(automation));
        _outputHelper = outputHelper ?? throw new ArgumentNullException(nameof(outputHelper));
        _vscWebSocketAutomation = new VisualStudioWebSocketAutomation(wsUri, _outputHelper);
    }

    public VisualStudioCodeWindow Window => _window ??= WaitForWindow();

    public void OpenFile(string filename)
    {
        ApplicationFactory.OpenFileInVsCode(filename);
        // var app = Application.Launch(
        //     @"""" + _exe + @"""",
        //     "--reuse-window ");
        // // "--new-window  --disable-extensions");
    }
    public Task<string> ExecuteCommand<T>(T command) where T : VscCommand
    {
        return _vscWebSocketAutomation.ExecuteCommandAsync(command);
    }

    public VisualStudioCodeWindow WaitForWindow()
    {
        if (_window != null)
        {
            return _window;
        }

        CancellationToken ct = new CancellationTokenSource(TimeSpan.FromSeconds(10)).Token;
        _app.WaitWhileMainHandleIsMissing(TimeSpan.FromSeconds(20));
        _app.WaitWhileBusy(TimeSpan.FromSeconds(20));
        _vscWebSocketAutomation.ConnectAsync(ct).GetAwaiter().GetResult();
        _vscWebSocketAutomation.StartProcessing(CancellationToken.None);
        return new VisualStudioCodeWindow(
            _app.GetMainWindow(_automation).FrameworkAutomationElement,
            _vscWebSocketAutomation,
            _outputHelper);
    }
    
    public void Dispose()
    {
        _vscWebSocketAutomation.Dispose();
        _app.Dispose();
    }
}