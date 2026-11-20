namespace RepoM.Plugin.WorktreePreview;

using System.IO.Abstractions;
using System.Reflection;
using System.Threading.Tasks;
using RepoM.Api.Plugins;
using RepoM.Core.StartupPlugin;
using RepoM.Plugin.Demo;
using SimpleInjector;

/// <summary>Uses the isolated demo infrastructure with the separate worktree preview window.</summary>
public sealed class WorktreePreviewStartupPlugin : IStartupPlugin
{
    public async Task RegisterStartupDependenciesAsync(Container container)
    {
        await new DemoStartupPlugin().RegisterStartupDependenciesAsync(container).ConfigureAwait(false);
        container.RegisterDecorator<IPluginFinder, WorktreePreviewPluginFinderDecorator>(Lifestyle.Singleton);
    }
}

public sealed class WorktreePreviewPluginFinderDecorator(
    IPluginFinder decoratee, IFileSystem fileSystem, IHmacService hmacService)
    : DemoPluginFinderDecorator(decoratee, fileSystem, hmacService)
{
    protected override Assembly PackageAssembly => typeof(WorktreePreviewStartupPlugin).Assembly;
}
