namespace RepoM.Plugin.Demo;

using System;
using System.IO.Abstractions;
using System.Threading.Tasks;
using RepoM.Core.Plugin.Common;
using RepoM.Core.StartupPlugin;

public sealed class DemoPathProviderFactoryDecorator : IConfigBasedAppDataPathProviderFactory
{
    private readonly IFileSystem _fileSystem;

    public DemoPathProviderFactoryDecorator(IConfigBasedAppDataPathProviderFactory decoratee, IFileSystem fileSystem)
    {
        ArgumentNullException.ThrowIfNull(decoratee);
        _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
    }

    public async Task<IAppDataPathProvider> CreateAsync()
    {
        var paths = DemoProfile.CreatePaths(_fileSystem);
        await DemoProfile.PrepareAsync(_fileSystem, paths).ConfigureAwait(false);
        return paths;
    }
}
