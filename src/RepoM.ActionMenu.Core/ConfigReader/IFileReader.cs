namespace RepoM.ActionMenu.Core.ConfigReader;

using System.Collections.Generic;
using System.Threading.Tasks;
using RepoM.ActionMenu.Core.Yaml.Model;

internal interface IFileReader
{
    Task<ActionMenuRoot?> DeserializeRoot(string filename);

    Task<TagsRoot?> DeserializeTagsRoot(string filename);

    Task<ContextRoot?> DeserializeContextRoot(string filename);

    Task<IDictionary<string, string>?> ReadEnvAsync(string filename);
}