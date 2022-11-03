namespace RepoM.Plugin.SonarCloud;

using System;
using ExpressionStringEvaluator.Methods;
using JetBrains.Annotations;

[UsedImplicitly]
internal class SonarCloudIsFavoriteMethod : IMethod
{
    private readonly SonarCloudFavoriteService _service;

    public SonarCloudIsFavoriteMethod(SonarCloudFavoriteService service)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
    }

    public bool CanHandle(string method)
    {
        return "SonarCloud.IsFavorite".Equals(method, StringComparison.CurrentCultureIgnoreCase);
    }

    public object? Handle(string method, params object?[] args)
    {
        if (args.Length != 1)
        {
            // not sure if we shouldn't throw.
            return null;
        }

        if (args[0] is not string key)
        {
            // not sure if we shouldn't throw.
            return null;
        }

        return _service.IsFavorite(key);
    }
}