namespace RepoM.ActionMenu.Core.Model;

using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using System.Threading.Tasks;
using RepoM.ActionMenu.Core.ActionMenu.Context;
using RepoM.ActionMenu.Core.Misc;
using RepoM.ActionMenu.Core.Yaml.Model.ActionContext;
using RepoM.ActionMenu.Core.Yaml.Model.Tags;
using RepoM.ActionMenu.Interface.ActionMenuFactory;
using RepoM.ActionMenu.Interface.Scriban;
using RepoM.ActionMenu.Interface.UserInterface;
using RepoM.ActionMenu.Interface.YamlModel;
using RepoM.ActionMenu.Interface.YamlModel.ActionMenus;
using Scriban;
using FileFunctions = RepoM.ActionMenu.Core.ActionMenu.Context.FileFunctions;
using IRepository = RepoM.Core.Plugin.Repository.IRepository;
using RepositoryFunctions = RepoM.ActionMenu.Core.ActionMenu.Context.RepositoryFunctions;

internal class ActionMenuGenerationContext : TemplateContext, IActionMenuGenerationContext, IContextMenuActionMenuGenerationContext
{
    private readonly ITemplateParser _templateParser;
    private readonly ITemplateContextRegistration[] _functionsArray;
    private readonly IActionMenuDeserializer _deserializer;
    private readonly IActionToRepositoryActionMapper[] _repositoryActionMappers;
    private readonly IContextActionProcessor[] _contextActionMappers;
    private RepoMScriptObject _rootScriptObject = null!; // used for cloning.

    public ActionMenuGenerationContext(
        ITemplateParser templateParser, 
        IFileSystem fileSystem,
        ITemplateContextRegistration[] functionsArray,
        IActionToRepositoryActionMapper[] repositoryActionMappers,
        IActionMenuDeserializer deserializer,
        IContextActionProcessor[] contextActionMappers)
    {
        _templateParser = templateParser ?? throw new ArgumentNullException(nameof(templateParser));
        _functionsArray = functionsArray ?? throw new ArgumentNullException(nameof(functionsArray));
        FileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
        _repositoryActionMappers = repositoryActionMappers ?? throw new ArgumentNullException(nameof(repositoryActionMappers));
        _deserializer = deserializer ?? throw new ArgumentNullException(nameof(deserializer));
        _contextActionMappers = contextActionMappers ?? throw new ArgumentNullException(nameof(contextActionMappers));
    }
    
    public IFileSystem FileSystem { get; }

    public DisposableContextScriptObject RepositoryActionsScriptContext { get; private set; } = null!;

    public IRepository Repository { get; private set;  } = null!;

    private EnvSetScriptObject? _env;

    public EnvSetScriptObject Env => _env ??= (EnvSetScriptObject)_rootScriptObject["env"];

    public async Task AddRepositoryContextAsync(Context? reposContext)
    {
        if (reposContext == null)
        {
            return;
        }

        foreach (IContextAction contextAction in reposContext)
        {
            await RepositoryActionsScriptContext.AddContextActionAsync(contextAction).ConfigureAwait(false);
        }
    }

    public async IAsyncEnumerable<UserInterfaceRepositoryActionBase> AddActionMenusAsync(List<IMenuAction>? menus)
    {
        if (menus == null)
        {
            yield break;
        }

        using IScope disposable = CreateGlobalScope();

        foreach (IMenuAction action in menus)
        {
            foreach (UserInterfaceRepositoryActionBase item in await AddMenuActionAsync(action).ConfigureAwait(false))
            {
                yield return item;
            }
        }
    }

    public IActionMenuGenerationContext Clone()
    {
        // this method doesn't work yet. Cloning the full Template context is not possible.
        // to be implemented (https://github.com/coenm/RepoM/issues/85)
        /*
        var repoMScriptObject = (RepoMScriptObject)_rootScriptObject.Clone(true);

        IScriptObject e = ((EnvSetScriptObject)_rootScriptObject["env"]).Clone(true);
        repoMScriptObject.SetValue("env", e, false);
        */
        var result = new ActionMenuGenerationContext(
            _templateParser,
            FileSystem,
            _functionsArray,
            _repositoryActionMappers,
            _deserializer,
            _contextActionMappers);

        result.Initialize(Repository);

        return result;
    }

    internal void Initialize(IRepository repository)
    {
        Repository = repository ?? throw new ArgumentNullException(nameof(repository));

        _rootScriptObject = CreateAndInitRepoMScriptObject(Repository);
        
        foreach (ITemplateContextRegistration contextRegistration in _functionsArray)
        {
            contextRegistration.RegisterFunctions(Decorate<ActionMenuGenerationContext>(_rootScriptObject));
        }

        PushGlobal(_rootScriptObject);
        RepositoryActionsScriptContext = new DisposableContextScriptObject(this, Env, _contextActionMappers);
        PushGlobal(RepositoryActionsScriptContext);
    }

    private static RepoMScriptObject CreateAndInitRepoMScriptObject(IRepository repository)
    {
        var scriptObj = new RepoMScriptObject();

        scriptObj.SetValue("file", new FileFunctions(), true);
        scriptObj.SetValue("repository", new RepositoryFunctions(repository), true);

        scriptObj.Add("env", new EnvSetScriptObject(EnvScriptObject.Create()));
        scriptObj.SetReadOnly("env", false); // this is not what we want, but it's the only way to make it work

        return scriptObj;
    }

    private async Task<IEnumerable<UserInterfaceRepositoryActionBase>> AddMenuActionAsync(IMenuAction menuAction)
    {
        if (!await IsMenuItemActiveAsync(menuAction).ConfigureAwait(false))
        {
            return Array.Empty<UserInterfaceRepositoryActionBase>();
        }

        using DisposableContextScriptObject variableContext = PushNewContext();

        if (menuAction is IContext { Context: not null, } c)
        {
            foreach (IContextAction ctx in c.Context)
            {
                await variableContext.AddContextActionAsync(ctx).ConfigureAwait(false);
            }
        }

        IActionToRepositoryActionMapper? mapper = Array.Find(_repositoryActionMappers, mapper => mapper.CanMap(menuAction));
        if (mapper == null)
        {
            // throw?
            return Array.Empty<UserInterfaceRepositoryActionBase>();
        }

        var items = new List<UserInterfaceRepositoryActionBase>();
        await foreach (UserInterfaceRepositoryActionBase item in mapper.MapAsync(menuAction, this, Repository).ConfigureAwait(false))
        {
            items.Add(item);
        }

        return items;
    }

    public async Task<string> RenderStringAsync(string text)
    {
        Template template = _templateParser.ParseMixed(text);
        return await template.RenderAsync(this).ConfigureAwait(false);
    }

    private DisposableContextScriptObject PushNewContext()
    {
        return new DisposableContextScriptObject(this, Env, _contextActionMappers);
    }

    public async Task<object> EvaluateAsync(string? text)
    {
        if (text == null)
        {
            return null!;
        }

        Template template = _templateParser.ParseScriptOnly(text);
        return await template.EvaluateAsync(this).ConfigureAwait(false);
    }
    
    private Task<bool> IsMenuItemActiveAsync(IMenuAction menuAction)
    {
        return menuAction.Active.EvaluateAsync(this);
    }

    public IScope CreateGlobalScope()
    {
        return PushNewContext();
    }

    public async Task<IEnumerable<string>> GetTagsAsync(Tags tags)
    {
        if (tags == null)
        {
            return Array.Empty<string>();
        }

        var items = new List<string>(Tags.Count);

        using IScope disposable = CreateGlobalScope();

        foreach (ITag tag in tags)
        {
            if (await tag.When.EvaluateAsync(this).ConfigureAwait(false))
            {
                items.Add(tag.Tag);
            }
        }

        return items.Distinct();
    }

    private static ContextRegistrationDecorator<T> Decorate<T>(IContextRegistration rootScriptObject) where T : TemplateContext
    {
        return new ContextRegistrationDecorator<T>(rootScriptObject);
    }
}