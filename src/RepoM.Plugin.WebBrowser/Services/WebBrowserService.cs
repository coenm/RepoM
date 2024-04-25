namespace RepoM.Plugin.WebBrowser.Services;

using System;
using Microsoft.Extensions.Logging;
using RepoM.Api.IO;

internal class WebBrowserService : IWebBrowserService
{
    private readonly WebBrowserConfiguration _configuration;
    private readonly ILogger _logger;

    public WebBrowserService(WebBrowserConfiguration configuration, ILogger logger)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _logger = logger;
    }

    public bool ProfileExist(string name)
    {
        return _configuration.Profiles.ContainsKey(name);
    }

    public void OpenUrl(string url)
    {
        StartProcess(url, string.Empty);
    }

    public void OpenUrl(string url, string profile)
    {

        if (!_configuration.Profiles.TryGetValue(profile, out BrowserProfileConfig? profileConfig))
        {
            OpenUrl(url);
            return;
        }

        if (!_configuration.Browsers.TryGetValue(profileConfig.BrowserName!, out string? browser))
        {
            OpenUrl(url);
            return;
        }
        
        if (string.IsNullOrWhiteSpace(profileConfig.CommandLineArguments))
        {
            StartProcess(browser, url);
            return;
        }

        var commandLinesArgs = profileConfig.CommandLineArguments;
        if (commandLinesArgs.Contains("{url}"))
        {
            commandLinesArgs = commandLinesArgs.Replace("{url}", url);
        }
        else
        {
            commandLinesArgs += " " + url;
        }

        StartProcess(browser, commandLinesArgs);
    }

    // virtual because of testing
    protected virtual void StartProcess(string process, string arguments)
    {
        ProcessHelper.StartProcess(process, arguments, _logger);
    }
}