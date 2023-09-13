namespace RepoM.Plugin.WebBrowser.Tests;

using System;
using FluentAssertions;
using RepoM.Plugin.WebBrowser.Services;
using VerifyXunit;
using Xunit;

[UsesVerify]
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
}