namespace RepoM.Plugin.EverythingFileSearch;

using System;
using JetBrains.Annotations;
using RepoM.Api.IO;
using RepoM.Plugin.EverythingFileSearch.Internal;

[UsedImplicitly]
public class EverythingGitRepositoryFinderFactory : ISingleGitRepositoryFinderFactory
{
    private readonly IPathSkipper _pathSkipper;

    private readonly Lazy<bool> _isInstalled = new(Everything64Api.IsInstalled);

    public EverythingGitRepositoryFinderFactory(IPathSkipper pathSkipper)
    {
        _pathSkipper = pathSkipper ?? throw new ArgumentNullException(nameof(pathSkipper));
    }

    public string Name { get; } = "Everything";

    public bool IsActive => _isInstalled.Value;

    public IGitRepositoryFinder Create()
    {
        return new EverythingGitRepositoryFinder(_pathSkipper);
    }
}