namespace RepoM.Plugin.Heidi.Internal.Config;

internal struct HeidiSingleDatabaseConfiguration
{
    public string Key { get; set; }

    public string Host { get; set; }

    public string User { get; set; }

    public string EncryptedPassword { get; set; }

    public string Port { get; set; }

    public int WindowsAuth { get; set; }

    public int NetType { get; set; }

    public string Library { get; set; }

    public string Comment { get; set; }

    public string Databases { get; set; }
}