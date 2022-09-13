namespace Tests.Git;

using FluentAssertions;
using NUnit.Framework;
using RepoM.Api.Git;
using Tests.Helper;

public class RepositoryTests
{
    private readonly RepositoryBuilder _builder1 = new();
    private readonly RepositoryBuilder _builder2 = new();

    public class EqualsMethod : RepositoryTests
    {
        [Test]
        public void Is_Not_Case_Sensitive()
        {
            Repository r1 = _builder1.WithPath(@"C:\Develop\RepoM\RepoM\").Build();
            Repository r2 = _builder2.WithPath(@"c:\develop\repom\rEPOm\").Build();

            r1.Equals(r2).Should().BeTrue();
        }

        [Test]
        public void Ignores_Ending_Slash()
        {
            Repository r1 = _builder1.WithPath(@"/c/develop/repom/repom").Build();
            Repository r2 = _builder2.WithPath(@"/c/develop/repom/repom/").Build();

            r1.Equals(r2).Should().BeTrue();
        }

        [Test]
        public void Ignores_Ending_Slashes()
        {
            Repository r1 = _builder1.WithPath(@"/c/develop/repom/repom/").Build();
            Repository r2 = _builder2.WithPath(@"/c/develop/repom/repom///").Build();

            r1.Equals(r2).Should().BeTrue();
        }

        [Test]
        public void Ignores_Ending_Backslash()
        {
            Repository r1 = _builder1.WithPath(@"C:\Develop\RepoM\RepoM").Build();
            Repository r2 = _builder2.WithPath(@"C:\Develop\RepoM\RepoM\").Build();

            r1.Equals(r2).Should().BeTrue();
        }

        [Test]
        public void Ignores_Ending_Backslashes()
        {
            Repository r1 = _builder1.WithPath(@"C:\Develop\RepoM\RepoM\").Build();
            Repository r2 = _builder2.WithPath(@"C:\Develop\RepoM\RepoM\\\").Build();

            r1.Equals(r2).Should().BeTrue();
        }

        [Test]
        public void Can_Use_Either_Slashes_Or_Backslashes()
        {
            Repository r1 = _builder1.WithPath(@"C:\Develop\RepoM\RepoM\").Build();
            Repository r2 = _builder2.WithPath(@"C:/Develop/RepoM/RepoM/").Build();

            r1.Equals(r2).Should().BeTrue();
        }

        [Test]
        public void Accepts_Empty_Strings()
        {
            Repository r1 = _builder1.WithPath(string.Empty).Build();
            Repository r2 = _builder2.WithPath(string.Empty).Build();

            r1.Equals(r2).Should().BeTrue();
        }

        [Test]
        public void Can_Handle_Null()
        {
            Repository r1 = _builder1.WithPath(@"C:\Develop\RepoM\RepoM\").Build();

            r1.Equals(null!).Should().BeFalse();
        }
    }
}