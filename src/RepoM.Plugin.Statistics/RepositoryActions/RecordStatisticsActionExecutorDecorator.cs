namespace RepoM.Plugin.Statistics.RepositoryActions;

using System;
using JetBrains.Annotations;
using RepoM.Core.Plugin.Repository;
using RepoM.Core.Plugin.RepositoryActions;
using RepoM.Core.Plugin.RepositoryActions.Actions;

[UsedImplicitly]
public sealed class RecordStatisticsActionExecutorDecorator<T> : IActionExecutor<T> where T : IAction
{
    private readonly IActionExecutor<T> _decoratee;
    private readonly IStatisticsService _service;

    public RecordStatisticsActionExecutorDecorator(
        IActionExecutor<T> decoratee,
        IStatisticsService service)
    {
        _decoratee = decoratee ?? throw new ArgumentNullException(nameof(decoratee));
        _service = service ?? throw new ArgumentNullException(nameof(service));
    }

    public void Execute(IRepository repository, T action)
    {
        if (action is not NullAction)
        {
            Record(repository);
        }

        _decoratee.Execute(repository, action);
    }

    private void Record(IRepository repository)
    {
        try
        {
            _service.Record(repository);
        }
        catch (Exception)
        {
            // swallow
        }
    }
}