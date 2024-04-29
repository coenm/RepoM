namespace RepoM.Api.Git;

using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using Microsoft.Extensions.Logging;

public abstract class FileRepositoryStore : IRepositoryStore
{
    private readonly IFileSystem _fileSystem;
    private readonly ILogger _logger;

    protected FileRepositoryStore(IFileSystem fileSystem, ILogger logger)
    {
        _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    protected abstract string GetFileName();

    public IEnumerable<string> Get()
    {
        var file = GetFileName();
        return Get(file);
    }

    public void Set(IEnumerable<string> paths)
    {
        var file = GetFileName();
        var path = _fileSystem.Directory.GetParent(file)?.FullName;
        _logger.LogDebug("Start File Repository Store {Method} - {Path}", nameof(Set), path);

        if (string.IsNullOrWhiteSpace(path))
        {
            return;
        }

        if (!_fileSystem.Directory.Exists(path))
        {
            _fileSystem.Directory.CreateDirectory(path);
        }

        try
        {
            _fileSystem.File.WriteAllLines(GetFileName(), paths.ToArray());
        }
        catch (Exception)
        {
            // swallow for now.
        }
    }

    private IEnumerable<string> Get(string file)
    {
        _logger.LogDebug("Start File Repository Store {Method} - {File}", nameof(Get), file);
        if (!_fileSystem.File.Exists(file))
        {
            return [];
        }

        try
        {
            return _fileSystem.File.ReadAllLines(file);
        }
        catch (Exception)
        {
            // swallow for now.
        }

        return [];
    }
}