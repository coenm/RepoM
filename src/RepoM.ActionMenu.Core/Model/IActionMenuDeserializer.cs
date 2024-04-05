namespace RepoM.ActionMenu.Core.Model;

using RepoM.ActionMenu.Core.Yaml.Model;

internal interface IActionMenuDeserializer
{
    string Serialize<T>(T actionMenuRoot) where T : ContextRoot;

    T Deserialize<T>(string content) where T : ContextRoot;
}