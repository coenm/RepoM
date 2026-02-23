namespace RepoM.Api.RepositoryActions.Executors;

using System;
using JetBrains.Annotations;
using RepoM.Core.Plugin.Repository;
using RepoM.Core.Plugin.RepositoryActions;
using RepoM.Core.Plugin.RepositoryActions.Commands;
using RepoM.Core.Repositories.Pinning;

[UsedImplicitly]
public sealed class PinRepositoryCommandExecutor : ICommandExecutor<PinRepositoryCommand>
{
    private readonly IPinningService _pinningService;

    public PinRepositoryCommandExecutor(IPinningService pinningService)
    {
        _pinningService = pinningService ?? throw new ArgumentNullException(nameof(pinningService));
    }

    public void Execute(IRepository repository, PinRepositoryCommand repositoryCommand)
    {
        var newPinnedValue = repositoryCommand.Type == PinRepositoryCommand.PinRepositoryType.Pin;
        newPinnedValue |= repositoryCommand.Type == PinRepositoryCommand.PinRepositoryType.Toggle && !_pinningService.IsPinned(repository.SafePath);
        _pinningService.SetPinned(repository.SafePath, newPinnedValue);
    }
}
