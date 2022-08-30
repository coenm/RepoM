namespace RepoM.Core.Plugin;

using System.Threading.Tasks;

public interface IModule
{
    Task StartAsync();

    Task StopAsync();
}