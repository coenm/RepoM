namespace RepoM.Api.Tests.Git;

using System.IO.Abstractions.TestingHelpers;
using FakeItEasy;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using RepoM.Api.Git;
using RepoM.Core.Plugin.Common;
using Xunit;

public class DefaultRepositoryIgnoreStoreTests
{
    private readonly DefaultRepositoryIgnoreStore _sut;

    public DefaultRepositoryIgnoreStoreTests()
    {
        IAppDataPathProvider appDataPathProvider = A.Fake<IAppDataPathProvider>();
        A.CallTo(() => appDataPathProvider.AppDataPath).Returns(""); //dummy value
        _sut = new DefaultRepositoryIgnoreStore(appDataPathProvider, new MockFileSystem(), NullLogger.Instance);

        _sut.IgnoreByPath(@"C:\data\repos\first");
        _sut.IgnoreByPath(@"C:\data\repox*");
        _sut.IgnoreByPath(@"*repoy*");
        _sut.IgnoreByPath(@"*repom\sub");
        _sut.IgnoreByPath(@"a*c");
    }
    
    [Fact]
    public void Ignores_Exact_Matches()
    {
        _sut.IsIgnored(@"C:\data\repos\first").Should().BeTrue();
    }

    [Fact]
    public void Ignores_Matches_With_Different_Casing()
    {
        _sut.IsIgnored(@"C:\data\REPOS\first").Should().BeTrue();
    }

    [Fact]
    public void Does_Not_Ignore_Subfolders_Of_Ignored_Folders()
    {
        _sut.IsIgnored(@"C:\data\REPOS\first\subfolder").Should().BeFalse();
    }

    [Fact]
    public void Respects_Wildcards_At_Start()
    {
        _sut.IsIgnored(@"C:\data\repom").Should().BeFalse();
        _sut.IsIgnored(@"C:\data\repom\").Should().BeFalse();
        _sut.IsIgnored(@"C:\data\repom\sub").Should().BeTrue();
        _sut.IsIgnored(@"C:\data\repom\sub\first").Should().BeFalse();

        _sut.IsIgnored(@"C:\DATA\repom\sub").Should().BeTrue();
    }

    [Fact]
    public void Respects_Wildcards_At_End()
    {
        _sut.IsIgnored(@"C:\data\repo").Should().BeFalse();
        _sut.IsIgnored(@"C:\data\repox").Should().BeTrue();
        _sut.IsIgnored(@"C:\data\repox\").Should().BeTrue();
        _sut.IsIgnored(@"C:\data\repox\first\subfolder").Should().BeTrue();

        _sut.IsIgnored(@"C:\DATA\repox\").Should().BeTrue();
    }

    [Fact]
    public void Respects_Wildcards_At_Start_And_End()
    {
        _sut.IsIgnored(@"C:\data\repoy").Should().BeTrue();
        _sut.IsIgnored(@"C:\data\repoy\first\subfolder").Should().BeTrue();
        _sut.IsIgnored(@"repoy\first\subfolder").Should().BeTrue();

        _sut.IsIgnored(@"C:\DATA\repoy").Should().BeTrue();
    }

    [Fact]
    public void Ignores_Wildcards_Within_Strings()
    {
        _sut.IsIgnored(@"abc").Should().BeFalse();
    }
}