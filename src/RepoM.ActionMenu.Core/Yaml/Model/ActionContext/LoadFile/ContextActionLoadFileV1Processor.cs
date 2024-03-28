namespace RepoM.ActionMenu.Core.Yaml.Model.ActionContext.LoadFile;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RepoM.ActionMenu.Core.ConfigReader;
using RepoM.ActionMenu.Interface.ActionMenuFactory;
using RepoM.ActionMenu.Interface.YamlModel;

internal class ContextActionLoadFileV1Processor : ContextActionProcessorBase<ContextActionLoadFileV1>
{
    private readonly IFileReader _fileReader;

    public ContextActionLoadFileV1Processor(IFileReader fileReader)
    {
        _fileReader = fileReader ?? throw new ArgumentNullException(nameof(fileReader));
    }

    protected override async Task ProcessAsync(ContextActionLoadFileV1 contextV1, IContextMenuActionMenuGenerationContext context, IScope scope)
    {
        var filename = await contextV1.Filename.RenderAsync(context).ConfigureAwait(false);

        if (string.IsNullOrWhiteSpace(filename))
        {
            return;
        }

        if (filename.EndsWith(".yml", StringComparison.CurrentCultureIgnoreCase) ||
            filename.EndsWith(".yaml", StringComparison.CurrentCultureIgnoreCase))
        {
            ContextRoot? contextRoot = await _fileReader.DeserializeContextRoot(filename).ConfigureAwait(false);

            if (contextRoot?.Context is null)
            {
                return;
            }

            foreach (IContextAction item in contextRoot.Context)
            {
                await scope.AddContextActionAsync(item).ConfigureAwait(false);
            }

            return;
        }

        if (filename.EndsWith(".env", StringComparison.CurrentCultureIgnoreCase))
        {
            IDictionary<string, string>? fileContents = await _fileReader.ReadEnvAsync(filename).ConfigureAwait(false);

            if (fileContents == null || fileContents.Count == 0)
            {
                return;
            }

            scope.PushEnvironmentVariable(fileContents);
        }

        // invalid extension.
    }
}