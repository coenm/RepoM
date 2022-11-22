namespace Yaml.Poc.Tests.DI;

using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using System.Threading;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using RepoM.Core.Plugin.RepositoryOrdering;
using SimpleInjector;
public interface IQuery<T1>
{
}

[DebuggerDisplay("{QueryType.Name,nq}")]
public sealed class QueryInfo
{
    public QueryInfo(Type queryType)
    {
        QueryType = queryType;
        ResultType = DetermineResultTypes(queryType).Single();
    }

    public Type QueryType { get; }

    public Type ResultType { get; }

    public static bool IsQuery(Type type)
    {
        return DetermineResultTypes(type).Any();
    }

    private static IEnumerable<Type> DetermineResultTypes(Type type)
    {
        return from interfaceType in type.GetInterfaces()
               where interfaceType.IsGenericType
               where interfaceType.GetGenericTypeDefinition() == typeof(IQuery<>)
               select interfaceType.GetGenericArguments()[0];
    }
}

public class QueryProcessor 
{
    private readonly Container _container;

    public QueryProcessor(Container container)
    {
        this._container = container;
    }

    public async Task<TResult> ExecuteQueryAsync<TResult>(IQuery<TResult> query, CancellationToken ct = default)
    {
        Type handlerType = typeof(IRepositoryScoreCalculatorFactory<>).MakeGenericType(query.GetType());

        dynamic handler = _container.GetInstance(handlerType);

        return await handler.HandleAsync((dynamic)query, null, ct);
    }
}