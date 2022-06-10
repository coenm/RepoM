namespace Specs.Mocks;

using RepoM.Api.IO;

internal class NeverSkippingPathSkipper : IPathSkipper
{
    public bool ShouldSkip(string path)
    {
        return false;
    }
}