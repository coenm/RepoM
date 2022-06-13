namespace Grr.App.Messages.Filters;

using System;
using Grr.App.History;

public class GoBackMessageFilter : IMessageFilter
{
    private readonly IHistoryRepository _historyRepository;

    public GoBackMessageFilter(IHistoryRepository historyRepository)
    {
        _historyRepository = historyRepository ?? throw new ArgumentNullException(nameof(historyRepository));
    }

    public void Filter(RepositoryFilterOptions filter)
    {
        var filterValue = filter?.RepositoryFilter ?? string.Empty;

        if ("-" == filterValue)
        {
            State state = _historyRepository.Load();
            if (filter != null)
            {
                filter.RepositoryFilter = state.LastLocation ?? filterValue;
            }
        }
    }
}