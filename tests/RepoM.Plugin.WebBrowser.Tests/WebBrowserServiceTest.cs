namespace RepoM.Plugin.WebBrowser.Tests;

using System;
using System.Collections.Generic;
using FluentAssertions;
using RepoM.Plugin.WebBrowser.Services;
using Xunit;

public class WebBrowserServiceTest
{
    [Fact]
    public void Ctor_ShouldThrow_WhenArgumentNull()
    {
        // arrange

        // act
        Func<WebBrowserService> act1 = () => new WebBrowserService(null!);

        // assert
        act1.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void ProfileExist_ShouldReturnFalse_WhenNoProfilesDefined()
    {

        // arrange
        var config = new WebBrowserConfiguration();
        var sut = new WebBrowserService(config);

        // act
        var result = sut.ProfileExist("name");

        // assert
        _ = result.Should().BeFalse();
    }

    [Fact]
    public void ProfileExist_ShouldReturnFalse_WhenNoProfileNameDoesNotMatchCase()
    {

        // arrange
        var config = new WebBrowserConfiguration
            {
                Profiles = new()
                    {
                        { "Name", new BrowserProfileConfig() },
                    },
            };
        var sut = new WebBrowserService(config);

        // act
        var result = sut.ProfileExist("name");

        // assert
        _ = result.Should().BeFalse();
    }

    [Fact]
    public void ProfileExist_ShouldReturnTrue_WhenProfileExists()
    {

        // arrange
        var config = new WebBrowserConfiguration
            {
                Profiles = new()
                    {
                        { "name1", new BrowserProfileConfig() },
                        { "name2", new BrowserProfileConfig() },
                        { "name3", new BrowserProfileConfig() },
                    },
            };
        var sut = new WebBrowserService(config);

        // act
        var result = sut.ProfileExist("name2");

        // assert
        _ = result.Should().BeTrue();
    }

    [Fact]
    public void OpenUrl_ShouldStartProcess()
    {
        // arrange
        var config = new WebBrowserConfiguration
            {
                Profiles = new()
                    {
                        { "name1", new BrowserProfileConfig() },
                        { "name2", new BrowserProfileConfig() },
                        { "name3", new BrowserProfileConfig() },
                    },
            };
        var sut = new DummyWebBrowserService(config);

        // act
        sut.OpenUrl("https://google.com");

        // assert
        sut.StartProcessCalled.Should().BeEquivalentTo("https://google.com - ");
    }

    [Fact]
    public void OpenUrl_WithProfile_ShouldStartProcess()
    {
        // arrange
        var config = new WebBrowserConfiguration
            {
                Profiles = new()
                    {
                        { "Private", new BrowserProfileConfig { BrowserName = "Edge", CommandLineArguments = "\"--profile 23 \" {url}", } },
                    },
                Browsers = new()
                    {
                        { "Edge", "msedge.exe" },
                    },
            };
        var sut = new DummyWebBrowserService(config);

        // act
        sut.OpenUrl("https://google.com", "Private");

        // assert
        sut.StartProcessCalled.Should().BeEquivalentTo("msedge.exe - \"--profile 23 \" https://google.com");
    }

    [Fact]
    public void OpenUrl_WithProfile_ShouldStartProcessWithoutProfile_WhenProfileNotExists()
    {
        // arrange
        var config = new WebBrowserConfiguration
            {
                Profiles = new()
                    {
                        { "Private", new BrowserProfileConfig { BrowserName = "Edge", CommandLineArguments = "\"--profile 23 \" {url}", } },
                    },
                Browsers = new()
                    {
                        { "Edge", "msedge.exe" },
                    },
            };
        var sut = new DummyWebBrowserService(config);

        // act
        sut.OpenUrl("https://google.com", "Private-Not-Exists");

        // assert
        sut.StartProcessCalled.Should().BeEquivalentTo("https://google.com - ");
    }

    [Fact]
    public void OpenUrl_WithProfile_ShouldStartProcessWithoutProfile_WhenBrowserNotExists()
    {
        // arrange
        var config = new WebBrowserConfiguration
            {
                Profiles = new()
                    {
                        { "Private", new BrowserProfileConfig { BrowserName = "InvalidBrowser", CommandLineArguments = "\"--profile 23 \" {url}", } },
                    },
                Browsers = new()
                    {
                        { "Edge", "msedge.exe" },
                    },
            };
        var sut = new DummyWebBrowserService(config);

        // act
        sut.OpenUrl("https://google.com", "Private");

        // assert
        sut.StartProcessCalled.Should().BeEquivalentTo("https://google.com - ");
    }
}

file class DummyWebBrowserService : WebBrowserService
{
    public DummyWebBrowserService(WebBrowserConfiguration configuration) : base(configuration)
    {
    }

    public List<string> StartProcessCalled { get; } = new(1);

    protected override void StartProcess(string process, string arguments)
    {
        StartProcessCalled.Add(process + " - " + arguments);
    }
}