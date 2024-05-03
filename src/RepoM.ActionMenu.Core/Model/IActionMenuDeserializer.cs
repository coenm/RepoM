namespace RepoM.ActionMenu.Core.Model;

using RepoM.ActionMenu.Core.Yaml.Model;

internal interface IActionMenuDeserializer
{
    T Deserialize<T>(string content) where T : ContextRoot;
}