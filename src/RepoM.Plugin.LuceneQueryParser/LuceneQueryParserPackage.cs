namespace RepoM.Plugin.LuceneQueryParser;

using System.Threading.Tasks;
using JetBrains.Annotations;
using RepoM.Core.Plugin;
using RepoM.Core.Plugin.RepositoryFiltering;
using SimpleInjector;

[UsedImplicitly]
public class LuceneQueryParserPackage : IPackage
{
    public string Name => "LuceneQueryParserPackage";

    public Task RegisterServicesAsync(Container container, IPackageConfiguration packageConfiguration)
    {
        RegisterPluginHooks(container);
        return Task.CompletedTask;
    }

    private static void RegisterPluginHooks(Container container)
    {
        container.Collection.Append<INamedQueryParser, LuceneQueryParser>(Lifestyle.Singleton);
    }
}