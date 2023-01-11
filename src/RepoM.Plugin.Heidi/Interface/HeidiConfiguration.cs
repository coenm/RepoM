namespace RepoM.Plugin.Heidi.Interface;

using System;
using System.Linq;
using RepoM.Plugin.Heidi.Internal.Config;

public class HeidiConfiguration
{
    internal HeidiConfiguration(RepoHeidi singleDatabaseConfiguration, HeidiSingleDatabaseConfiguration heidiSingleDatabaseConfiguration, string heidiFilename)
    : this(
        singleDatabaseConfiguration.Name,
        singleDatabaseConfiguration.Order,
        singleDatabaseConfiguration.Tags.ToArray(),
        heidiFilename,
        new HeidiDbConfig
            {
                Key = heidiSingleDatabaseConfiguration.Key,
                User = heidiSingleDatabaseConfiguration.User,
                Password = heidiSingleDatabaseConfiguration.Password,
                Port = heidiSingleDatabaseConfiguration.Port,
                WindowsAuth = heidiSingleDatabaseConfiguration.WindowsAuth,
                NetType = heidiSingleDatabaseConfiguration.NetType,
                Library = heidiSingleDatabaseConfiguration.Library,
                Comment = heidiSingleDatabaseConfiguration.Comment,
                Databases = heidiSingleDatabaseConfiguration.Databases.ToArray(),
                Host = heidiSingleDatabaseConfiguration.Host,
            })
    {
    }

    public HeidiConfiguration(string name, int order, string[] tags, string heidiFilename, HeidiDbConfig heidiDbConfig)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Order = order;
        Tags = tags ?? throw new ArgumentNullException(nameof(tags));
        HeidiDb = heidiDbConfig ?? throw new ArgumentNullException(nameof(heidiDbConfig));
        HeidiFilename = heidiFilename ?? throw new ArgumentNullException(nameof(heidiFilename));
    }

    public HeidiDbConfig HeidiDb { get; }

    public int Order { get; }

    public string Name { get; } 

    public string[] Tags { get; }

    public string HeidiFilename { get; }
}