namespace RepoM.Plugin.Heidi.Internal;

using System.Diagnostics.CodeAnalysis;
using RepoM.Plugin.Heidi.Internal.Config;

internal interface IHeidiRepositoryExtractor
{
    bool TryExtract(HeidiSingleDatabaseConfiguration config, [NotNullWhen(true)] out RepoHeidi? output);
}