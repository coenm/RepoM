namespace RepoM.Plugin.LuceneQueryParser;

using JetBrains.Annotations;
using RepoM.Core.Plugin.RepositoryFiltering;
using SimpleInjector;
using SimpleInjector.Packaging;

[UsedImplicitly]
public class LuceneQueryParserPackage : IPackage
{
    public void RegisterServices(Container container)
    {
        RegisterPluginHooks(container);
    }

    private static void RegisterPluginHooks(Container container)
    {
        container.Collection.Append<INamedQueryParser, LuceneQueryParser>(Lifestyle.Singleton);
    }
}