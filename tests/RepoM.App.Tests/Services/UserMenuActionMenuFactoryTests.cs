namespace RepoM.App.Tests.Services;

using System;
using System.IO.Abstractions;
using FakeItEasy;
using FluentAssertions;
using RepoM.ActionMenu.Core;
using RepoM.App.Services;
using RepoM.Core.Plugin.Common;
using Xunit;

public class UserMenuActionMenuFactoryTests
{
    [Fact]
    public void Ctor_ShouldThrow_WhenArgumentNull()
    {
        // arrange

        // act
        Func<UserMenuActionMenuFactory> act1 = () => new UserMenuActionMenuFactory(A.Dummy<IFileSystem>(), A.Dummy<IAppDataPathProvider>(), null!);
        Func<UserMenuActionMenuFactory> act2 = () => new UserMenuActionMenuFactory(A.Dummy<IFileSystem>(), null!, A.Dummy<IUserInterfaceActionMenuFactory>());
        Func<UserMenuActionMenuFactory> act3 = () => new UserMenuActionMenuFactory(null!, A.Dummy<IAppDataPathProvider>(), A.Dummy<IUserInterfaceActionMenuFactory>());

        // assert
        act1.Should().Throw<ArgumentNullException>();
        act2.Should().Throw<ArgumentNullException>();
        act3.Should().Throw<ArgumentNullException>();
    }
}