namespace RepoM.ActionMenu.Core.Tests.ActionMenu.Context;

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using System.Threading.Tasks;
using FakeItEasy;
using FluentAssertions;
using RepoM.ActionMenu.Core.TestLib;
using RepoM.ActionMenu.Interface.ActionMenuFactory;
using RepoM.Core.Plugin.Repository;
using Scriban.Parsing;
using Xunit;
using Xunit.Categories;
using Sut = RepoM.ActionMenu.Core.ActionMenu.Context.FileFunctions;

public class FileFunctionsTests
{
    private readonly IMenuContext _context;
    private readonly SourceSpan _span = new("", TextPosition.Eof, TextPosition.Eof);
    private readonly IFileSystem _fileSystem;

    public FileFunctionsTests()
    {
        _context = A.Fake<IMenuContext>();
        A.CallTo(() => _context.Repository).Returns(A.Fake<IRepository>());
        _fileSystem = A.Fake<IFileSystem>();
        A.CallTo(() => _context.FileSystem).Returns(_fileSystem);
    }

    [Fact]
    public void FindFiles_ShouldReturnEmpty_WhenNoFilesFound()
    {
        // arrange
        var path = "my-path";
        var search = "my-search";
        var files = Array.Empty<IFileSystemInfo>();
        IDirectoryInfo di = A.Fake<IDirectoryInfo>();
        A.CallTo(() => _fileSystem.DirectoryInfo.New(path)).Returns(di);
        A.CallTo(() => di.EnumerateFileSystemInfos(search, SearchOption.AllDirectories))
         .Returns(files);

        // act
        IEnumerable result = Sut.FindFilesInner(_context, _span, path, search);

        // assert
        result.Should().BeEquivalentTo(Array.Empty<string>());
        A.CallTo(() => _fileSystem.DirectoryInfo.New(path)).MustHaveHappenedOnceExactly();
        A.CallTo(() => di.EnumerateFileSystemInfos(search, SearchOption.AllDirectories)).MustHaveHappenedOnceExactly();
    }
    
    [Theory]
    [InlineData(@"C:\Project")]
    [InlineData(@"C:\Project\")]
    [InlineData(@"C:\Project/")]
    [InlineData(@"c:\Project\")]
    [InlineData(@"C:/Project")]
    [InlineData(@"C:/Project/")]
    [InlineData(@"C:/Project\")]
    public void FindFiles_ShouldReturnFiles_WhenFilesAvailable(string path)
    {
        // arrange
        var rootPath = Path.Combine("C:", "Project", "My Repositories");
        var fs = new MockFileSystem();
        fs.AddDirectory(Path.Combine(rootPath, "src"));
        fs.AddFile(Path.Combine(rootPath, "my-solution.sln"), new MockFileData("dummy"));
        fs.AddFile(Path.Combine(rootPath, "src", "test solution.sln"), new MockFileData("dummy"));
        fs.AddFile(Path.Combine(rootPath, "src", "dummy.txt"), new MockFileData("dummy"));
        A.CallTo(() => _context.FileSystem).Returns(fs);

        // act
        IEnumerable result = Sut.FindFilesInner(_context, _span, path, "*.sln");

        // assert
        result.Should().BeEquivalentTo(new List<string>
            {
                @"C:\Project\My Repositories\my-solution.sln",
                @"C:\Project\My Repositories\src\test solution.sln",
            });
    }

    [Fact]
    [Documentation]
    public async Task FindFiles_Documentation()
    {
        // arrange
        var rootPath = Path.Combine("C:", "Project", "My Repositories");
        var fs = new MockFileSystem();
        fs.AddDirectory(Path.Combine(rootPath, "src"));
        fs.AddFile(Path.Combine(rootPath, "my-solution.sln"), new MockFileData("dummy"));
        fs.AddFile(Path.Combine(rootPath, "src", "test solution.sln"), new MockFileData("dummy"));
        fs.AddFile(Path.Combine(rootPath, "src", "dummy.txt"), new MockFileData("dummy"));
        A.CallTo(() => _context.FileSystem).Returns(fs);

        // act
        IEnumerable result = Sut.FindFilesInner(_context, _span, @"C:\Project\", "*.sln");

        // assert
        result.Should().BeEquivalentTo(new List<string>
            {
                @"C:\Project\My Repositories\my-solution.sln",
                @"C:\Project\My Repositories\src\test solution.sln",
            });

        // assert
        await DocumentationGeneration
              .CreateAndVerifyDocumentation(result)
              .UseFileName("file.find_files");
    }
}