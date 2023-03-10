namespace RepoM.Plugin.Statistics.VariableProviders;

using System;
using System.Linq;
using JetBrains.Annotations;
using RepoM.Core.Plugin.Repository;
using RepoM.Core.Plugin.VariableProviders;

[UsedImplicitly]
public class UsageVariableProvider : IVariableProvider<RepositoryContext>
{
    private const StringComparison COMPARISON = StringComparison.CurrentCultureIgnoreCase;
    private readonly IStatisticsService _service;

    public UsageVariableProvider(IStatisticsService service)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
    }

    public bool CanProvide(string key)
    {
       
        if (string.IsNullOrWhiteSpace(key))
        {
            return false;
        }

        if (key.Equals("usage", COMPARISON))
        {
            return true;
        }

        if (key.Equals("statistics.totalcount", COMPARISON))
        {
            return true;
        }

        if (key.Equals("statistics.count", COMPARISON))
        {
            return true;
        }

        return false;
    }

    public object? Provide(RepositoryContext context, string key, string? arg)
    {
        if ("statistics.count".Equals(key, COMPARISON) || "usage".Equals(key, COMPARISON))
        {
            return _service.GetRecordings(context.Repositories.First()).Count;
        }

        if ("statistics.totalcount".Equals(key, COMPARISON))
        {
            return _service.GetTotalRecordingCount();
        }

        throw new NotImplementedException();
    }

    public object? Provide(string key, string? arg)
    {
        throw new NotImplementedException();
    }
}