namespace RepoM.Api.QuickFilter;

using System;
using System.Collections.Generic;
using RepoM.Core.Plugin.RepositoryFiltering.Clause;

public enum QuickFilterCombineMode
{
    And,
    Or,
}

public interface IQuickFilterService
{
    QuickFilterCombineMode CombineMode { get; set; }

    IReadOnlyList<QuickFilterModel> GetAll();

    QuickFilterModel Add(string label, IQuery query);

    void Remove(Guid id);

    void SetActive(Guid id, bool isActive);

    void SetInverse(Guid id, bool isInverse);

    void UpdateLabel(Guid id, string newLabel);

    void UpdateToolTip(Guid id, string newToolTip);

    void UpdateOrder(Guid id, int newOrder);

    QuickFilterModel? FindByQuery(IQuery query);

    event EventHandler? Changed;
}