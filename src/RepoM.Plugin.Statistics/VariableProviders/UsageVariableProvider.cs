namespace RepoM.Plugin.Statistics.VariableProviders;

using System;
using System.Linq;
using JetBrains.Annotations;
using RepoM.Core.Plugin.Repository;
using RepoM.Core.Plugin.VariableProviders;

[UsedImplicitly]
public class UsageVariableProvider : IVariableProvider<RepositoryContext>
{
    private readonly StatisticsService _service;

    public UsageVariableProvider(StatisticsService service)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
    }

    public bool CanProvide(string key)
    {
        return !string.IsNullOrWhiteSpace(key) && key.StartsWith("usage", StringComparison.CurrentCultureIgnoreCase);
    }

    public object? Provide(RepositoryContext context, string key, string? arg)
    {
        return _service.GetRecordings(context.Repositories.First()).Count;
    }

    public object? Provide(string key, string? arg)
    {
        throw new NotImplementedException();
    }
}