namespace RepoM.Api.Resources;

using System.IO;
using System.Reflection;
using LibGit2Sharp;

internal static class EmbeddedResources
{
    private static readonly Assembly _assembly = typeof(EmbeddedResources).Assembly;
    private static readonly string _namespace = typeof(EmbeddedResources).Namespace!;

    public static Stream GetRepositoryActionsV2Yaml()
    {
        return ResolveFromAssembly("RepositoryActionsV2.yaml");
    }

    public static Stream GetSortingYaml()
    {
        return ResolveFromAssembly("RepoM.Sorting.yaml");
    }

    public static Stream GetFilteringYaml()
    {
        return ResolveFromAssembly("RepoM.Filtering.yaml");
    }

    public static Stream GetSerilogAppSettings()
    {
        return ResolveFromAssembly("appsettings.serilog.json");
    }

    private static Stream ResolveFromAssembly(string relativeFilename)
    {
        var embeddedFilename = $"{_namespace}.{relativeFilename}";
        return _assembly.GetManifestResourceStream(embeddedFilename) ?? throw new NotFoundException($"{relativeFilename} not found.");
    }
}