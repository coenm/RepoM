namespace RepoM.Plugin.Heidi.Internal.Config;

// internal class RepomHeidiConfig
// {
//     public string HeidiKey { get; set; }
//
//     public string[] Repositories { get; set; }
//
//     public int Order { get; set; }
//
//     public string Name { get; set; }
//
//     public string Environment { get; set; }
// }

/// <summary>
/// Model corresponding with config file
/// </summary>
internal class HeidiDatabaseConfiguration
{
    public string Key { get; set; }

    public string Host { get; set; }

    public string User { get; set; }

    /// <summary>
    /// Encrypted EncryptedPassword
    /// </summary>
    public string Password { get; set; }

    public int Port { get; set; }

    public bool WindowsAuth { get; set; }

    public int NetType { get; set; }

    public string Library { get; set; }

    public string Comment { get; set; }

    public string Databases { get; set; }
}