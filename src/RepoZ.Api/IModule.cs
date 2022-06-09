namespace RepoZ.Api;

using System.Threading.Tasks;
using JetBrains.Annotations;

public interface IModule
{
    Task StartAsync();

    Task StopAsync();
}