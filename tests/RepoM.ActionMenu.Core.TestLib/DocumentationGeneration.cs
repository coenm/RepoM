namespace RepoM.ActionMenu.Core.TestLib;

using System.IO;
using VerifyTests;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

public static class DocumentationGeneration
{
    public static string CreateDocumentationYaml<TModel>(TModel model)
    {
        return new SerializerBuilder()
               .WithNamingConvention(HyphenatedNamingConvention.Instance)
               .Build()
               .Serialize(model);
    }

    public static VerifySettings GetVerifySettings()
    {
        var dir = Path.Combine("..", "..", "..", "..", "docs", "snippets");
        var settings = new VerifySettings();
        settings.UseDirectory(dir);
        return settings;
    }
}