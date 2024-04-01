namespace RepoM.Api.Resources;

using System.IO;
using System.Linq;
using System.Reflection;
using LibGit2Sharp;

internal static class EmbeddedResources
{
    private static readonly Assembly _assembly = typeof(EmbeddedResources).Assembly;
    private static readonly string _namespace = typeof(EmbeddedResources).Namespace!;

    public static Stream GetRepositoryActionsV2Yaml()
    {
        return ResolveFromAssembly("RepositoryActionsV2.yaml") ?? throw new NotFoundException("File not found.");
    }

    private static Stream? ResolveFromAssembly(string relativeFilename)
    {
        var embeddedFilename = $"{_namespace}.{relativeFilename}";
        return _assembly.GetManifestResourceStream(embeddedFilename);
    }
}