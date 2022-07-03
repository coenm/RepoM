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

    public CombinedTypeContainer Handle(string method, params CombinedTypeContainer[] args)
    {
        if (args.Length != 1)
        {
            // not sure if we shouldn't throw.
            return CombinedTypeContainer.NullInstance;
        }

        CombinedTypeContainer keyArg = args[0];
        if (!keyArg.IsString(out var key))
        {
            // not sure if we shouldn't throw.
            return CombinedTypeContainer.NullInstance;
        }

        return _service.IsFavorite(key)
            ? CombinedTypeContainer.TrueInstance
            : CombinedTypeContainer.FalseInstance;
    }
}