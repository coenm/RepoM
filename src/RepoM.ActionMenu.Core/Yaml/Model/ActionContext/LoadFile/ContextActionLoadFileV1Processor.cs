namespace RepoM.ActionMenu.Core.Yaml.Model.ActionContext.LoadFile;

using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Threading.Tasks;
using DotNetEnv;
using RepoM.ActionMenu.Core.Model;
using RepoM.ActionMenu.Interface.ActionMenuFactory;
using RepoM.ActionMenu.Interface.YamlModel;

internal class ContextActionLoadFileV1Processor : ContextActionProcessorBase<ContextActionLoadFileV1>
{
    private readonly IFileSystem _fileSystem;
    private readonly IActionMenuDeserializer _deserializer;

    public ContextActionLoadFileV1Processor(IFileSystem fileSystem, IActionMenuDeserializer deserializer)
    {
        _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
        _deserializer = deserializer ?? throw new ArgumentNullException(nameof(deserializer));
    }

    protected override async Task ProcessAsync(ContextActionLoadFileV1 contextV1, IContextMenuActionMenuGenerationContext context, IScope scope)
    {
        var filename = await contextV1.Filename.RenderAsync(context).ConfigureAwait(false);

        if (string.IsNullOrWhiteSpace(filename))
        {
            return;
        }

        if (!_fileSystem.File.Exists(filename))
        {
            return;
        }

        if (filename.EndsWith(".yml", StringComparison.CurrentCultureIgnoreCase) ||
            filename.EndsWith(".yaml", StringComparison.CurrentCultureIgnoreCase))
        {
            var yaml = await _fileSystem.File.ReadAllTextAsync(filename).ConfigureAwait(false);
            ContextRoot contextRoot = _deserializer.DeserializeContextRoot(yaml);

            if (contextRoot.Context is not null)
            {
                foreach (IContextAction item in contextRoot.Context)
                {
                    await scope.AddContextActionAsync(item).ConfigureAwait(false);
                }
            }
            
            return;
        }

        if (filename.EndsWith(".env", StringComparison.CurrentCultureIgnoreCase))
        {
            var envContent = await _fileSystem.File.ReadAllTextAsync(filename).ConfigureAwait(false);
            IEnumerable<KeyValuePair<string, string>>? envResult = Env.LoadContents(envContent, new LoadOptions(setEnvVars: false));

            if (envResult == null)
            {
                return;
            }
            
            var fileContents = envResult.ToDictionary();
            if (fileContents.Count == 0)
            {
                return;
            }

            scope.PushEnvironmentVariable(fileContents);
        }

        // invalid extension.
        return;
    }
}