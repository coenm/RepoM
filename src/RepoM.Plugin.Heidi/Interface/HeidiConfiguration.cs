namespace RepoM.Plugin.Heidi.Interface;

using System;

public class HeidiConfiguration
{
    public HeidiConfiguration(string name, string description, int order, string? environment)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Description = description ?? throw new ArgumentNullException(nameof(description));
        Order = order;
        Environment = environment ?? string.Empty;
    }

    public string Name { get; }

    public string Description { get; }
    
    public int Order { get; }
    
    public string Environment { get; }
}