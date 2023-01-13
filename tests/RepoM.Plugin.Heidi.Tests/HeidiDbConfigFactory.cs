namespace RepoM.Plugin.Heidi.Tests;

using System.Linq;
using RepoM.Plugin.Heidi.Interface;

internal static class HeidiDbConfigFactory
{
    public static HeidiDbConfig CreateHeidiDbConfig(int i, string key = "")
    {
        return new HeidiDbConfig
            {
                Comment = $"comment{i}",
                Databases = Enumerable.Range(0, i).Select(x => $"db{x}").ToArray(),
                Host = $"Host{i}",
                Key = string.IsNullOrWhiteSpace(key) ? $"Key{i}" : key,
                WindowsAuth = i % 2 == 0,
                Library = $"lib{i}",
                NetType = i + 1,
                Password = $"pwd{i}pwd",
                Port = 100 + i,
                User = $"usr{i}",
            };
    }

}