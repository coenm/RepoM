namespace RepoM.Api.Git;

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Threading;
using RepoM.Core.Plugin.Common;

public class DefaultRepositoryIgnoreStore : FileRepositoryStore, IRepositoryIgnoreStore
{
    private List<string>? _ignores;
    private IEnumerable<IgnoreRule>? _rules;
    private readonly Lock _lock = new();
    private readonly string _fullFilename;

    public DefaultRepositoryIgnoreStore(
        IAppDataPathProvider appDataPathProvider,
        IFileSystem fileSystem)
        : base(fileSystem)
    {
        _ = appDataPathProvider ?? throw new ArgumentNullException(nameof(appDataPathProvider));
        _fullFilename = Path.Combine(appDataPathProvider.AppDataPath, "Repositories.ignore");
    }

    protected override string GetFileName()
    {
        return _fullFilename;
    }

    public void IgnoreByPath(string path)
    {
        Ignores.Add(path);
        UpdateRules();

        Set(Ignores);
    }

    public bool IsIgnored(string path)
    {
        if (_rules is null)
        {
            UpdateRules();
        }

        return _rules?.Any(r => r.IsIgnored(path)) ?? false;
    }

    public void Reset()
    {
        Ignores.Clear();
        UpdateRules();

        Set(Ignores);
    }

    private List<string> Ignores
    {
        get
        {
            if (_ignores != null)
            {
                return _ignores;
            }

            lock (_lock)
            {
                if (_ignores != null)
                {
                    return _ignores;
                }

                _ignores = Get().ToList();
                UpdateRules();
            }

            return _ignores;
        }
    }

    private void UpdateRules()
    {
        _rules = Ignores.Select(i => new IgnoreRule(i));
    }
}