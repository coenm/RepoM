namespace RepoM.ActionMenu.Core.TestLib;

using System;
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

    public static SettingsTask CreateAndVerifyYamlSnippet<TModel>(TModel data, string snippetKey, [CallerFilePath] string sourceFile = "")
    {
        var yaml = $"# begin-snippet: {snippetKey}{Environment.NewLine}{Environment.NewLine}" +
                   $"{_serializer.Serialize(data)}{Environment.NewLine}{Environment.NewLine}" +
                   $"# end-snippet";
        
        return VerifyYaml(yaml, sourceFile);
    }

    public static SettingsTask CreateAndVerifyDocumentation<TModel>(TModel data, [CallerFilePath] string sourceFile = "")
    {
        var yaml = _serializer.Serialize(data);
        return VerifyYaml(yaml, sourceFile);
    }
   
    private static SettingsTask VerifyYaml(string yaml, string sourceFile = "")
    {
        // ReSharper disable once ExplicitCallerInfoArgument
        #pragma warning disable S3236
        return Verifier.Verify(yaml, extension: "yaml", sourceFile: sourceFile);
        #pragma warning restore S3236
    }
}