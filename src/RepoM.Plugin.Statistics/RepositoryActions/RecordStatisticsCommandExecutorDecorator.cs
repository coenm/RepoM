namespace RepoM.Plugin.Statistics.RepositoryActions;

using System;
using JetBrains.Annotations;
using RepoM.Core.Plugin.Repository;
using RepoM.Core.Plugin.RepositoryActions;
using RepoM.Core.Plugin.RepositoryActions.Commands;

[UsedImplicitly]
internal sealed class RecordStatisticsCommandExecutorDecorator<T> : ICommandExecutor<T> where T : IRepositoryCommand
{
    private readonly ICommandExecutor<T> _decoratee;
    private readonly IStatisticsService _service;

    public RecordStatisticsCommandExecutorDecorator(
        ICommandExecutor<T> decoratee,
        IStatisticsService service)
    {
        _decoratee = decoratee ?? throw new ArgumentNullException(nameof(decoratee));
        _service = service ?? throw new ArgumentNullException(nameof(service));
    }

    public void Execute(IRepository repository, T action)
    {
        if (action is not NullRepositoryCommand)
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