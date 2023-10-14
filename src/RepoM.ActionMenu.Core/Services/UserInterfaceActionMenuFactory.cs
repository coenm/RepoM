namespace RepoM.ActionMenu.Core.Services;

using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Threading.Tasks;
using RepoM.ActionMenu.Core.Misc;
using RepoM.ActionMenu.Core.Model;
using RepoM.ActionMenu.Core.PublicApi;
using RepoM.ActionMenu.Interface.Scriban;
using RepoM.ActionMenu.Interface.UserInterface;
using RepoM.Core.Plugin.Repository;

internal class UserInterfaceActionMenuFactory : IUserInterfaceActionMenuFactory
{
    private readonly IFileSystem _fileSystem;
    private readonly ITemplateParser _templateParser;
    private readonly ITemplateContextRegistration[] _plugins;
    private readonly string _filename;

    public UserInterfaceActionMenuFactory(
        IFileSystem fileSystem, 
        ITemplateParser templateParser,
        ITemplateContextRegistration[] plugins,
        string filename)
    {
        _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
        _templateParser = templateParser ?? throw new ArgumentNullException(nameof(templateParser));
        _plugins = plugins ?? throw new ArgumentNullException(nameof(plugins));
        _filename = filename ?? throw new ArgumentNullException(nameof(filename));
    }

    public async Task<IEnumerable<UserInterfaceRepositoryActionBase>> CreateMenuAsync(IRepository repository)
    {
        var context = new ActionMenuGenerationContext(repository, _templateParser, _fileSystem, _plugins);

        // context.Repository.IsStarred = false;
        
        // load yaml
        var actions = await context.LoadAsync(_filename).ConfigureAwait(false);

        // process context (vars + methods)
        await context.AddRepositoryContextAsync(actions.Context).ConfigureAwait(false);

        // process tags

        // process actions
        return await context.AddActionMenusAsync(actions.ActionMenu).ConfigureAwait(false);
    }


    public async Task<IEnumerable<string>> GetTagsAsync(IRepository repository)
    {
        var context = new ActionMenuGenerationContext(repository, _templateParser, _fileSystem, _plugins);

        // load yaml
        var actions = await context.LoadAsync(_filename).ConfigureAwait(false);

        if (actions.Tags == null)
        {
            return Array.Empty<string>();
        }

        // process context (vars + methods)
        await context.AddRepositoryContextAsync(actions.Context).ConfigureAwait(false);

        // process tags
        return await context.GetTagsAsync(actions.Tags).ConfigureAwait(false);
    }
}