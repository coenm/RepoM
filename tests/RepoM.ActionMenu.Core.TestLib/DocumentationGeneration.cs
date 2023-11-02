namespace RepoM.ActionMenu.Core.TestLib;

using System.IO;
using System.Runtime.CompilerServices;
using VerifyTests;
using VerifyXunit;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

public static class DocumentationGeneration
{
    private static readonly ISerializer _serializer = new SerializerBuilder()
      .WithNamingConvention(HyphenatedNamingConvention.Instance)
      .Build();

    public static SettingsTask CreateAndVerifyDocumentation<TModel>(TModel data, [CallerFilePath] string sourceFile = "")
    {
        var yaml = _serializer.Serialize(data);
        return Verifier.Verify(yaml, settings: GetVerifySettings(), extension: "yaml", sourceFile: sourceFile);
    }

    private static VerifySettings GetVerifySettings()
    {
        var dir = Path.Combine("..", "..", "..", "..", "docs", "snippets");
        var settings = new VerifySettings();
        settings.UseDirectory(dir);
        return settings;
    }
}