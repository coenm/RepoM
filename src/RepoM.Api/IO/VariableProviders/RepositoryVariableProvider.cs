namespace RepoM.Api.IO.VariableProviders;

using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using RepoM.Core.Plugin.Repository;

public class RepositoryVariableProvider : RepoM.Core.Plugin.VariableProviders.IVariableProvider<RepositoryContext>
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
        return ProvideString(context, key);
    }

    private static string ProvideString(RepositoryContext context, string key)
    {
        IRepository? repository = context.Repository;
        if (repository == null)
        {
            return string.Empty;
        }

        var startIndex = "Repository.".Length;
        var keySuffix = key[startIndex..];

        if (TryProvideProperties(repository, keySuffix, out string? result))
        {
            return result;
        }

        // legacy
        if ("RemoteUrls".Equals(keySuffix, StringComparison.CurrentCultureIgnoreCase))
        {
            return string.Join("|", repository.Remotes.Select(x => x.Url));
        }
        
        if (keySuffix.StartsWith("Remote.", StringComparison.CurrentCultureIgnoreCase))
        {
            startIndex = "Remote.".Length;
            keySuffix = keySuffix[startIndex..];

            var splits = keySuffix.Split('.');
            if (splits.Length != 2)
            {
                return string.Empty;
            }

            if (TryProvideRemoteProperty(repository, splits[0], splits[1], out result))
            {
                return result;
            }

            return string.Empty;
        }

        throw new NotImplementedException();
    }

    private static bool TryProvideProperties(IRepository repository, string key, [NotNullWhen(true)] out string? result)
    {
        if ("Name".Equals(key, StringComparison.CurrentCultureIgnoreCase))
        {
            result = repository.Name;
            return true;
        }

        if ("Path".Equals(key, StringComparison.CurrentCultureIgnoreCase))
        {
            result = repository.Path;
            return true;
        }

        if ("SafePath".Equals(key, StringComparison.CurrentCultureIgnoreCase))
        {
            result = repository.SafePath;
            return true;
        }

        if ("Location".Equals(key, StringComparison.CurrentCultureIgnoreCase))
        {
            result = repository.Location;
            return true;
        }

        if ("CurrentBranch".Equals(key, StringComparison.CurrentCultureIgnoreCase))
        {
            result = repository.CurrentBranch;
            return true;
        }

        if ("Branches".Equals(key, StringComparison.CurrentCultureIgnoreCase))
        {
            result = string.Join("|", repository.Branches);
            return true;
        }

        if ("LocalBranches".Equals(key, StringComparison.CurrentCultureIgnoreCase))
        {
            result = string.Join("|", repository.LocalBranches);
            return true;
        }

        result = null;
        return false;
    }

    private static bool TryProvideRemoteProperty(IRepository repository, string remoteName, string property, [NotNullWhen(true)] out string? result)
    {
        Remote? remote = repository.Remotes.Find(x => x.Key.Equals(remoteName, StringComparison.CurrentCultureIgnoreCase));
        if (remote == null)
        {
            result =  string.Empty;
            return true;
        }

        if ("url".Equals(property, StringComparison.CurrentCultureIgnoreCase))
        {
            result =  remote.Url;
            return true;
        }

        if ("key".Equals(property, StringComparison.CurrentCultureIgnoreCase))
        {
            result = remote.Key;
            return true;
        }

        if ("name".Equals(property, StringComparison.CurrentCultureIgnoreCase))
        {
            result = remote.Name;
            return true;
        }

        result = null;
        return false;
    }
}