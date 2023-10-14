namespace RepoM.ActionMenu.Core.Yaml.Model.Ctx.LoadFile;

using System;
using System.IO.Abstractions;
using System.Threading.Tasks;
using DotNetEnv;
using RepoM.ActionMenu.Core.Model;
using RepoM.ActionMenu.Interface.ActionMenuFactory;

internal class LoadFileContextActionContextActionMapper : ContextActionMapperBase<LoadFileContextAction>
{
    private readonly IFileSystem _fileSystem;
    private readonly IActionMenuDeserializer _deserializer;

    public LoadFileContextActionContextActionMapper(IFileSystem fileSystem, IActionMenuDeserializer deserializer)
    {
        _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
        _deserializer = deserializer ?? throw new ArgumentNullException(nameof(deserializer));
    }

    protected override async Task MapAsync(LoadFileContextAction contextContextAction, IContextMenuActionMenuGenerationContext context, IScope scope)
    {
        var filename = await context.RenderNullableString(contextContextAction.Filename).ConfigureAwait(false);

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
            var contextRoot = _deserializer.DeserializeContextRoot(yaml);

            if (contextRoot.Context is not null)
            {
                foreach (var item in contextRoot.Context)
                {
                    await scope.AddContextActionAsync(item).ConfigureAwait(false);
                }
            }
            
            return;
        }

        if (filename.EndsWith(".env", StringComparison.CurrentCultureIgnoreCase))
        {
            var envContent = await _fileSystem.File.ReadAllTextAsync(filename).ConfigureAwait(false);
            var envResult = Env.LoadContents(envContent, new LoadOptions(setEnvVars: false));

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