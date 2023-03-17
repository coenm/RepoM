namespace SystemTests.Mocks;

using RepoM.Core.Plugin.RepositoryFinder;

internal class NeverSkippingPathSkipper : IPathSkipper
{
    public bool ShouldSkip(string path)
    {
        return false;
    }
}