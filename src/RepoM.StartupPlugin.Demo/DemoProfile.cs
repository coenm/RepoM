namespace RepoM.StartupPlugin.Demo;

using System;
using System.IO.Abstractions;
using System.Threading.Tasks;
using RepoM.Api.IO;

public static class DemoProfile
{
    /// <summary>Creates an isolated settings location, never the user's normal RepoM profile.</summary>
    public static AppDataPathProvider CreatePaths(IFileSystem fileSystem) => new(
        new AppDataPathConfig
        {
            AppSettingsPath = fileSystem.Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "RepoM", "Demo"),
        }, fileSystem);

    /// <summary>Installs a self-contained menu, without reading repository files or environment scripts.</summary>
    public static async Task PrepareAsync(IFileSystem fileSystem, AppDataPathProvider paths)
    {
        fileSystem.Directory.CreateDirectory(paths.AppDataPath);
        foreach (string filename in new[] { "RepositoryActionsV2.yaml", "RepoM.Filtering.yaml", "RepoM.Ordering.yaml" })
        {
            using var source = typeof(DemoProfile).Assembly.GetManifestResourceStream(
                $"RepoM.StartupPlugin.Demo.Resources.{filename}")
                ?? throw new InvalidOperationException($"The demo resource '{filename}' is missing.");
            await using var destination = fileSystem.File.Create(fileSystem.Path.Combine(paths.AppDataPath, filename));
            await source.CopyToAsync(destination).ConfigureAwait(false);
        }
    }
}
