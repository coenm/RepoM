namespace RepoM.Core.Plugin;

using System.Threading.Tasks;

public interface IPackageConfiguration
{
    Task<int?> GetConfigurationVersionAsync();

    Task<T?> LoadConfigurationAsync<T>() where T : class, new();

    Task PersistConfigurationAsync<T>(T configuration, int version);
}