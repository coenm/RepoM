namespace RepoM.Api.IO;

using System;
using System.IO.Abstractions;
using RepoM.Core.Plugin.RepositoryFinder;

public class GravellGitRepositoryFinderFactory : ISingleGitRepositoryFinderFactory
{
    private readonly IPathSkipper _pathSkipper;
    private readonly IFileSystem _fileSystem;

    public GravellGitRepositoryFinderFactory(IPathSkipper pathSkipper, IFileSystem fileSystem)
    {
        _pathSkipper = pathSkipper ?? throw new ArgumentNullException(nameof(pathSkipper));
        _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
    }

    public IGitRepositoryFinder Create()
    {
        return new GravellGitRepositoryFinder(_pathSkipper, _fileSystem);
    }
}