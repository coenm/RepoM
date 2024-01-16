namespace RepoM.ActionMenu.Core.TestLib;

using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using RepoM.ActionMenu.Core.TestLib.Utils;
using VerifyTests;
using VerifyXunit;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

public static class DocumentationGeneration
{
    private static readonly ISerializer _serializer = new SerializerBuilder()
      .WithNamingConvention(HyphenatedNamingConvention.Instance)
      .Build();

    private static readonly TestAssemblyInfo _info = new(typeof(DocumentationGeneration).Assembly);

    public static SettingsTask CreateAndVerifyYamlSnippet<TModel>(TModel data, string snippetKey, [CallerFilePath] string sourceFile = "")
    {
        var yaml = $"# begin-snippet: {snippetKey}{Environment.NewLine}{Environment.NewLine}" +
                   $"{_serializer.Serialize(data)}{Environment.NewLine}{Environment.NewLine}" +
                   $"# end-snippet";
        
        return Verifier.Verify(yaml, settings: GetVerifySettings(), extension: "yaml", sourceFile: sourceFile);
    }

    public static SettingsTask CreateAndVerifyDocumentation<TModel>(TModel data, [CallerFilePath] string sourceFile = "")
    {
        var yaml = _serializer.Serialize(data);
        return Verifier.Verify(yaml, settings: GetVerifySettings(), extension: "yaml", sourceFile: sourceFile);
    }

    [Obsolete]
    public static async Task<string> LoadYamlFileAsync(string filename)
    {
        var dir = _info.SolutionDirectory;
        if (string.IsNullOrWhiteSpace(dir))
        {
            throw new Exception("Could not grab solution directory");
        }

        dir = Path.Combine(dir, "docs", "snippets");
        if (!Directory.Exists(dir))
        {
            throw new DirectoryNotFoundException(dir);
        }

        var fullFilename = Path.Combine(dir, filename);
        if (!File.Exists(fullFilename))
        {
            throw new FileNotFoundException(fullFilename);
        }

        return await File.ReadAllTextAsync(fullFilename);
    }

    private static VerifySettings GetVerifySettings()
    {
        return new VerifySettings();
        // var dir = _info.SolutionDirectory;
        // if (string.IsNullOrWhiteSpace(dir))
        // {
        //     throw new Exception("Could not grab solution directory");
        // }
        // dir = Path.Combine(dir, "docs", "snippets");
        // var settings = new VerifySettings();
        // settings.UseDirectory(dir);
        // return settings;
    }
}