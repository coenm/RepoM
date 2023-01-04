namespace RepoM.Plugin.Heidi.Interface;

public class HeidiConfiguration
{
    public string Description { get; internal init; }

    public string Name { get; internal init; }

    public int Order { get; internal init; }
    
    public string Environment { get; internal init; }
}