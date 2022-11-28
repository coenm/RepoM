namespace RepoM.Api.IO.VariableProviders;

using System;
using System.Linq;
using ExpressionStringEvaluator.VariableProviders;
using RepoM.Core.Plugin.Repository;

public class RepositoryVariableProvider : IVariableProvider<RepositoryContext>
{
    public bool CanProvide(string key)
    {
        return !string.IsNullOrWhiteSpace(key) && key.StartsWith("Repository.", StringComparison.CurrentCultureIgnoreCase);
    }

    public object? Provide(string key, string? arg)
    {
        throw new NotImplementedException();
    }

    public object? Provide(RepositoryContext context, string key, string? arg)
    {
        return ProvideString(context, key, arg);
    }

    private static string ProvideString(RepositoryContext context, string key, string? arg)
    {
        IRepository? repository = context.Repositories.SingleOrDefault();
        if (repository == null)
        {
            return string.Empty;
        }

        var startIndex = "Repository.".Length;
        var keySuffix = key.Substring(startIndex, key.Length - startIndex);

        if ("Name".Equals(keySuffix, StringComparison.CurrentCultureIgnoreCase))
        {
            return repository.Name;
        }

        if ("Path".Equals(keySuffix, StringComparison.CurrentCultureIgnoreCase))
        {
            return repository.Path;
        }

        if ("SafePath".Equals(keySuffix, StringComparison.CurrentCultureIgnoreCase))
        {
            return repository.SafePath;
        }

        if ("Location".Equals(keySuffix, StringComparison.CurrentCultureIgnoreCase))
        {
            return repository.Location;
        }

        if ("CurrentBranch".Equals(keySuffix, StringComparison.CurrentCultureIgnoreCase))
        {
            return repository.CurrentBranch;
        }

        if ("Branches".Equals(keySuffix, StringComparison.CurrentCultureIgnoreCase))
        {
            return string.Join("|", repository.Branches);
        }

        if ("LocalBranches".Equals(keySuffix, StringComparison.CurrentCultureIgnoreCase))
        {
            return string.Join("|", repository.LocalBranches);
        }

        // legacy
        if ("RemoteUrls".Equals(keySuffix, StringComparison.CurrentCultureIgnoreCase))
        {
            return string.Join("|", repository.Remotes.Select(x => x.Url));
        }

        if (keySuffix.StartsWith("Remote.", StringComparison.CurrentCultureIgnoreCase))
        {
            var subKey = keySuffix.Substring(2);

            startIndex = "Remote.".Length;
            keySuffix = keySuffix.Substring(startIndex, keySuffix.Length - startIndex);

            var splits = keySuffix.Split('.');
            if (splits.Length != 2)
            {
                return string.Empty;
            }

            Remote? remote = repository.Remotes.FirstOrDefault(x => x.Key.Equals(splits[0], StringComparison.CurrentCultureIgnoreCase));
            if (remote == null)
            {
                return string.Empty;
            }

            if ("url".Equals(splits[1], StringComparison.CurrentCultureIgnoreCase))
            {
                return remote.Url;
            }

            if ("key".Equals(splits[1], StringComparison.CurrentCultureIgnoreCase))
            {
                return remote.Key;
            }

            if ("name".Equals(splits[1], StringComparison.CurrentCultureIgnoreCase))
            {
                return remote.Name;
            }

            return string.Empty;
        }

        throw new NotImplementedException();
    }
}