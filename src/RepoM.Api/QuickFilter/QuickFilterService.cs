namespace RepoM.Api.QuickFilter;

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RepoM.Core.Plugin.Common;
using RepoM.Core.Plugin.RepositoryFiltering.Clause;
using RepoM.Core.Plugin.RepositoryFiltering.Clause.Terms;

internal sealed class QuickFilterService : IQuickFilterService
{
    private readonly IFileSystem _fileSystem;
    private readonly ILogger _logger;
    private readonly string _filePath;
    private readonly List<QuickFilterModel> _filters;

    private static readonly JsonSerializerSettings _jsonSettings = new()
    {
        Formatting = Formatting.Indented,
        NullValueHandling = NullValueHandling.Ignore,
        Converters = { new QueryJsonConverter(), },
    };

    private static readonly Guid _builtInFavoriteId = new("00000000-0000-0000-0000-000000000001");
    private static readonly Guid _builtInActiveId = new("00000000-0000-0000-0000-000000000002");

    public QuickFilterService(IAppDataPathProvider appDataPathProvider, IFileSystem fileSystem, ILogger logger)
    {
        _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        ArgumentNullException.ThrowIfNull(appDataPathProvider);

        _filePath = Path.Combine(appDataPathProvider.AppDataPath, "quickfilters.json");
        _filters = Load();
        EnsureBuiltInFilters();
    }

    public event EventHandler? Changed;

    public IReadOnlyList<QuickFilterModel> GetAll()
    {
        return _filters.OrderBy(f => f.Order).ToList();
    }

    public QuickFilterModel Add(string label, IQuery query)
    {
        ArgumentNullException.ThrowIfNull(query);

        var model = new QuickFilterModel
        {
            Id = Guid.NewGuid(),
            Label = label ?? string.Empty,
            Query = query,
            IsActive = true,
            Order = _filters.Count > 0 ? _filters.Max(f => f.Order) + 1 : 0,
        };

        _filters.Add(model);
        Save();
        Changed?.Invoke(this, EventArgs.Empty);
        return model;
    }

    public void Remove(Guid id)
    {
        var index = _filters.FindIndex(f => f.Id == id);
        if (index < 0)
        {
            return;
        }

        if (_filters[index].IsBuiltIn)
        {
            return;
        }

        _filters.RemoveAt(index);
        Save();
        Changed?.Invoke(this, EventArgs.Empty);
    }

    public void SetActive(Guid id, bool isActive)
    {
        var filter = _filters.Find(f => f.Id == id);
        if (filter == null || filter.IsActive == isActive)
        {
            return;
        }

        filter.IsActive = isActive;
        Save();
        Changed?.Invoke(this, EventArgs.Empty);
    }

    public void SetInverse(Guid id, bool isInverse)
    {
        var filter = _filters.Find(f => f.Id == id);
        if (filter == null || filter.IsInverse == isInverse)
        {
            return;
        }

        filter.IsInverse = isInverse;
        Save();
        Changed?.Invoke(this, EventArgs.Empty);
    }

    public void UpdateLabel(Guid id, string newLabel)
    {
        var filter = _filters.Find(f => f.Id == id);
        if (filter == null || filter.IsBuiltIn)
        {
            return;
        }

        filter.Label = newLabel ?? string.Empty;
        Save();
        Changed?.Invoke(this, EventArgs.Empty);
    }

    public void UpdateToolTip(Guid id, string newToolTip)
    {
        var filter = _filters.Find(f => f.Id == id);
        if (filter == null || filter.IsBuiltIn)
        {
            return;
        }

        filter.ToolTip = newToolTip ?? string.Empty;
        Save();
        Changed?.Invoke(this, EventArgs.Empty);
    }

    public void UpdateOrder(Guid id, int newOrder)
    {
        var filter = _filters.Find(f => f.Id == id);
        if (filter == null || filter.IsBuiltIn)
        {
            return;
        }

        filter.Order = newOrder;
        Save();
        Changed?.Invoke(this, EventArgs.Empty);
    }

    public QuickFilterModel? FindByQuery(IQuery query)
    {
        var queryString = query.ToString();
        return _filters.Find(f => string.Equals(f.Query.ToString(), queryString, StringComparison.OrdinalIgnoreCase));
    }

    private List<QuickFilterModel> Load()
    {
        if (!_fileSystem.File.Exists(_filePath))
        {
            return [];
        }

        try
        {
            var json = _fileSystem.File.ReadAllText(_filePath);
            var result = JsonConvert.DeserializeObject<List<QuickFilterModel>>(json, _jsonSettings);
            return result ?? [];
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not load quick filters from '{File}'.", _filePath);
        }

        return [];
    }

    private void Save()
    {
        try
        {
            IDirectoryInfo? directoryInfo = _fileSystem.Directory.GetParent(_filePath);
            if (directoryInfo != null && !_fileSystem.Directory.Exists(directoryInfo.FullName))
            {
                _fileSystem.Directory.CreateDirectory(directoryInfo.FullName);
            }

            var userFilters = _filters.Where(f => !f.IsBuiltIn).OrderBy(f => f.Order).ToList();
            var json = JsonConvert.SerializeObject(userFilters, _jsonSettings);
            _fileSystem.File.WriteAllText(_filePath, json);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Could not save quick filters to '{File}'.", _filePath);
        }
    }

    private void EnsureBuiltInFilters()
    {
        if (_filters.All(f => f.Id != _builtInFavoriteId))
        {
            _filters.Add(new QuickFilterModel
            {
                Id = _builtInFavoriteId,
                Label = "\u2605",
                Query = new SimpleTerm("is", "favorite"),
                IsActive = false,
                Order = -2,
                IsBuiltIn = true,
            });
        }

        if (_filters.All(f => f.Id != _builtInActiveId))
        {
            _filters.Add(new QuickFilterModel
            {
                Id = _builtInActiveId,
                Label = "\uD83D\uDC41",
                Query = new SimpleTerm("is", "active"),
                IsActive = false,
                Order = -1,
                IsBuiltIn = true,
            });
        }
    }
}