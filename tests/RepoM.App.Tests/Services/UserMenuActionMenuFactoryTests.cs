namespace RepoM.App.Tests.Services;

using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using System.Threading.Tasks;
using FakeItEasy;
using FluentAssertions;
using RepoM.ActionMenu.Core;
using RepoM.ActionMenu.Interface.UserInterface;
using RepoM.App.Services;
using RepoM.Core.Plugin.Common;
using RepoM.Core.Plugin.Repository;
using SimpleInjector.Diagnostics;
using Xunit;

public class UserMenuActionMenuFactoryTests
{
    private IUserInterfaceActionMenuFactory _factory;
    private IAppDataPathProvider _appDataPathProvider;
    private IFileSystem _fileSystem;

    public UserMenuActionMenuFactoryTests()
    {
        _factory = A.Fake<IUserInterfaceActionMenuFactory>();
        _appDataPathProvider = A.Fake<IAppDataPathProvider>();
        _fileSystem = A.Fake<IFileSystem>();
    }

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

    [Fact]
    public async Task CreateMenuAsync_ShouldReturnErrorMenu_WhenFileNotExists()
    {
        // arrange
        A.CallTo(() => _fileSystem.File.Exists(A<string>._)).Returns(false);
        A.CallTo(() => _appDataPathProvider.AppDataPath).Returns("C:\\AppData");

        var sut = new UserMenuActionMenuFactory(_fileSystem, _appDataPathProvider, _factory);

        // act
        List<UserInterfaceRepositoryActionBase> result = await sut.CreateMenuAsync(A.Dummy<IRepository>()).ToListAsync();

        // assert
        result.Should().HaveCount(3);
        A.CallTo(_factory).MustNotHaveHappened();
    }
}