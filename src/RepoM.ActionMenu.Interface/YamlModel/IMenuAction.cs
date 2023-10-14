namespace RepoM.ActionMenu.Interface.YamlModel;

public interface IMenuAction
{
    string Type { get; }
    
    string? Active { get; }
}