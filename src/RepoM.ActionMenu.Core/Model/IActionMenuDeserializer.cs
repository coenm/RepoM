namespace RepoM.ActionMenu.Core.Model;

using RepoM.ActionMenu.Core.Yaml.Model;

internal interface IActionMenuDeserializer
{
    Root DeserializeRoot(string content);
    
    ContextRoot DeserializeContextRoot(string content);
}