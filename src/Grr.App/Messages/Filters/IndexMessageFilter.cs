namespace Grr.App.Messages.Filters;

using System;
using Grr.App.History;

public class IndexMessageFilter : IMessageFilter
{
    private readonly IHistoryRepository _historyRepository;

    public IndexMessageFilter(IHistoryRepository historyRepository)
    {
        _historyRepository = historyRepository ?? throw new ArgumentNullException(nameof(historyRepository));
    }

    public void Filter(RepositoryFilterOptions filter)
    {
        if (filter?.RepositoryFilter == null)
        {
            return;
        }

        if (!filter.RepositoryFilter.StartsWith(":"))
        {
            return;
        }

        var rest = filter.RepositoryFilter[1..];
        if (!int.TryParse(rest, out var index))
        {
            return;
        }

        index--; // the index visible to the user are 1-based, not 0-based
        State state = _historyRepository.Load();
        if (state.LastRepositories == null)
        {
            return;
        }

        if (index >= 0 && state.LastRepositories.Length > index)
        {
            filter.RepositoryFilter = state.LastRepositories[index]?.Name ?? filter.RepositoryFilter;
        }
    }
}