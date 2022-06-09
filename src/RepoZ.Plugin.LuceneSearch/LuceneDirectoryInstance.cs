namespace RepoZ.Plugin.LuceneSearch;

using System;
using Lucene.Net.Store;

internal class LuceneDirectoryInstance
{
    private readonly ILuceneDirectoryFactory _factory;
    private Directory? _instance;

    public LuceneDirectoryInstance(ILuceneDirectoryFactory factory)
    {
        _factory = factory ?? throw new ArgumentNullException(nameof(factory));
    }

    public Directory Instance => _instance ??= _factory.Create();
}