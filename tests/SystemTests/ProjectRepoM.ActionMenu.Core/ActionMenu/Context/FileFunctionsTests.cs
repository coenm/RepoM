namespace SystemTests.ProjectRepoM.ActionMenu.Core.ActionMenu.Context;

using System;
using System.IO;
using System.IO.Abstractions;
using FakeItEasy;
using FluentAssertions;
using RepoM.ActionMenu.Core.ActionMenu.Context;
using RepoM.ActionMenu.Interface.ActionMenuFactory;
using Scriban.Parsing;

public class FileFunctionsTests
{
    private readonly IFileSystem _fileSystem = new FileSystem();
    private readonly IMenuContext _context;
    private readonly SourceSpan _span;
    private readonly string _rootPath;
    private readonly string _rootPathSubDir;

    public FileFunctionsTests()
    {
        _context = A.Fake<IMenuContext>();
        A.CallTo(() => _context.FileSystem).Returns(_fileSystem);
        _span =  new SourceSpan("fileName", new TextPosition(1, 1, 1), new TextPosition(1, 1, 1));
        _rootPath = Path.Combine(Path.GetTempPath(), $"RepoM_Test_Repositories_{Guid.NewGuid().ToString()}");
        _rootPathSubDir = Path.Combine(_rootPath, "subDir");
    }

    [Before(Test)]
    public void Setup()
    {
        _fileSystem.Directory.CreateDirectory(_rootPath);
        using var _ = _fileSystem.File.Create(Path.Combine(_rootPath, "root.sln"));
        _fileSystem.Directory.CreateDirectory(_rootPathSubDir);
        using var __ = _fileSystem.File.Create(Path.Combine(_rootPathSubDir, "sub.sln"));
    }

    [After(Test)]
    public void Cleanup()
    {
        _fileSystem.Directory.Delete(_rootPath, true);
    }

    [Test]
    public void FindFiles_ShouldReturnRootAndSubDirFile()
    {
        // arrange

        // act
        var result = FileFunctions.FindFilesInner(_context, _span, _rootPath, "*.sln");

        // assert
        result.Should().BeEquivalentTo(
            $@"{_rootPath}\root.sln",
            $@"{_rootPathSubDir}\sub.sln");
    }
}