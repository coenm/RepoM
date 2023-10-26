namespace RepoM.App.ActionMenuCore;

using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using Microsoft.Extensions.Logging;
using RepoM.ActionMenu.Core;
using RepoM.Api.Git;
using RepoM.Api.IO.ModuleBasedRepositoryActionProvider;
using RepoM.Core.Plugin.Common;

public class RepositoryTagsFactoryV2Decorator : IRepositoryTagsFactory
{
    private readonly IRepositoryTagsFactory _inner;
    private readonly IUserInterfaceActionMenuFactory _newStyleActionMenuFactory;
    private readonly ILogger _logger;
    private readonly bool _exists;
    private readonly string _filename;

    public RepositoryTagsFactoryV2Decorator(
        IRepositoryTagsFactory inner,
        IFileSystem fileSystem,
        IUserInterfaceActionMenuFactory newStyleActionMenuFactory,
        IAppDataPathProvider appDataPathProvider,
        ILogger logger)
    {
        _inner = inner ?? throw new ArgumentNullException(nameof(inner));
        _ = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
        _newStyleActionMenuFactory = newStyleActionMenuFactory ?? throw new ArgumentNullException(nameof(newStyleActionMenuFactory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _ = appDataPathProvider ?? throw new ArgumentNullException(nameof(appDataPathProvider));

        _filename = fileSystem.Path.Combine(appDataPathProvider.AppDataPath, "RepositoryActionsV2.yaml");
        _exists = fileSystem.File.Exists(_filename);
    }

    public IEnumerable<string> GetTags(Repository repository)
    {
        string[] origResult = _inner.GetTags(repository).ToArray();
        string[] newResult = Array.Empty<string>();

        if (_exists)
        {
            try
            {
                newResult = _newStyleActionMenuFactory.GetTagsAsync(repository, _filename).GetAwaiter().GetResult().ToArray();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Could not get tags. {message}", e.Message);
                return _inner.GetTags(repository);
            }

            var same = (origResult.SequenceEqual(newResult));
            if (!same)
            {
                return newResult;
            }

            return newResult;
        }

        return origResult;
    }
}