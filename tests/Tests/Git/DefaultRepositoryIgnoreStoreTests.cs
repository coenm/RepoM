namespace Tests.Git;

using System.IO.Abstractions;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using RepoM.Api.Common;
using RepoM.Api.Common.Git;
using RepoM.Api.IO;

public class DefaultRepositoryIgnoreStoreTests
{
    private DefaultRepositoryIgnoreStore _store = null!;

    [OneTimeSetUp]
    public void Setup()
    {
        var appDataPathProvider = new Mock<IAppDataPathProvider>();
        appDataPathProvider.Setup(x => x.GetAppDataPath()).Returns(""); //dummy value
        _store = new DefaultRepositoryIgnoreStore(new Mock<IErrorHandler>().Object, appDataPathProvider.Object, new FileSystem())
            {
                UseFilePersistence = false, 
            };

        _store.IgnoreByPath(@"C:\data\repos\first");
        _store.IgnoreByPath(@"C:\data\repox*");
        _store.IgnoreByPath(@"*repoy*");
        _store.IgnoreByPath(@"*repom\sub");
        _store.IgnoreByPath(@"a*c");
    }

    public class IsIgnoredMethod : DefaultRepositoryIgnoreStoreTests
    {
        [Test]
        public void Ignores_Exact_Matches()
        {
            _store.IsIgnored(@"C:\data\repos\first").Should().BeTrue();
        }

        [Test]
        public void Ignores_Matches_With_Different_Casing()
        {
            _store.IsIgnored(@"C:\data\REPOS\first").Should().BeTrue();
        }

        [Test]
        public void Does_Not_Ignore_Subfolders_Of_Ignored_Folders()
        {
            _store.IsIgnored(@"C:\data\REPOS\first\subfolder").Should().BeFalse();
        }

        [Test]
        public void Respects_Wildcards_At_Start()
        {
            _store.IsIgnored(@"C:\data\repom").Should().BeFalse();
            _store.IsIgnored(@"C:\data\repom\").Should().BeFalse();
            _store.IsIgnored(@"C:\data\repom\sub").Should().BeTrue();
            _store.IsIgnored(@"C:\data\repom\sub\first").Should().BeFalse();

            _store.IsIgnored(@"C:\DATA\repom\sub").Should().BeTrue();
        }

        [Test]
        public void Respects_Wildcards_At_End()
        {
            _store.IsIgnored(@"C:\data\repo").Should().BeFalse();
            _store.IsIgnored(@"C:\data\repox").Should().BeTrue();
            _store.IsIgnored(@"C:\data\repox\").Should().BeTrue();
            _store.IsIgnored(@"C:\data\repox\first\subfolder").Should().BeTrue();

            _store.IsIgnored(@"C:\DATA\repox\").Should().BeTrue();
        }

        [Test]
        public void Respects_Wildcards_At_Start_And_End()
        {
            _store.IsIgnored(@"C:\data\repoy").Should().BeTrue();
            _store.IsIgnored(@"C:\data\repoy\first\subfolder").Should().BeTrue();
            _store.IsIgnored(@"repoy\first\subfolder").Should().BeTrue();

            _store.IsIgnored(@"C:\DATA\repoy").Should().BeTrue();
        }

        [Test]
        public void Ignores_Wildcards_Within_Strings()
        {
            _store.IsIgnored(@"abc").Should().BeFalse();
        }
    }
}