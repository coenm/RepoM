namespace RepoM.Api.IO.ModuleBasedRepositoryActionProvider.Deserialization;

using RepoM.Api.IO.ModuleBasedRepositoryActionProvider.Data;

public interface IRepositoryActionDeserializer
{
    RepositoryActionConfiguration Deserialize(string content);
}