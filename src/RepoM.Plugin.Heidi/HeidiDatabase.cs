namespace RepoM.Plugin.Heidi;

internal class HeidiDatabase
{
    public HeidiDatabase(string name)
    {
        Name = name;
    }

    public string Name { get; }
}