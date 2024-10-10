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
using Scriban.Runtime;
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
    private RepoMScriptObject _rootScriptObject = null!;
    private EnvSetScriptObject? _env;
    private FastStack<DisposableContextScriptObject> _globals = new(4);
    private DisposableContextScriptObject _repositoryActionsScriptContext = null!;

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

    public IRepository Repository { get; private set; } = null!;

    public IFileSystem FileSystem { get; }
    
    public EnvSetScriptObject Env => _env ??= (EnvSetScriptObject)_rootScriptObject["env"];

    public async Task AddRepositoryContextAsync(Context? reposContext)
    {
        if (reposContext == null)
        {
            return;
        }

        foreach (IContextAction contextAction in reposContext)
        {
            await _repositoryActionsScriptContext.AddContextActionAsync(contextAction).ConfigureAwait(false);
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

    public void PushGlobal(DisposableContextScriptObject scriptObject)
    {
        base.PushGlobal(scriptObject);
        _globals.Push(scriptObject);
    }

    public new void PopGlobal()
    {
        _globals.Pop();
        base.PopGlobal();
    }

    public IActionMenuGenerationContext Clone()
    {
        var result = new ActionMenuGenerationContext(
            _templateParser,
            FileSystem,
            _functionsArray,
            _repositoryActionMappers,
            _deserializer,
            _contextActionMappers);

        result.InitializeFrom(this);
        return result;
    }

    internal void Initialize(IRepository repository)
    {
        Repository = repository ?? throw new ArgumentNullException(nameof(repository));

        _rootScriptObject = CreateAndInitRepoMScriptObject(
            new EnvSetScriptObject(EnvScriptObject.Instance));

        foreach (ITemplateContextRegistration contextRegistration in _functionsArray)
        {
            contextRegistration.RegisterFunctions(Decorate<ActionMenuGenerationContext>(_rootScriptObject));
        }

        PushGlobal(_rootScriptObject);
        _repositoryActionsScriptContext = new DisposableContextScriptObject(this, _contextActionMappers);
        PushGlobal(_repositoryActionsScriptContext);
    }

    private void InitializeFrom(ActionMenuGenerationContext @this)
    {
        Repository = @this.Repository;

        _rootScriptObject = CreateAndInitRepoMScriptObject(@this.Env.Clone());
        foreach (ITemplateContextRegistration contextRegistration in _functionsArray)
        {
            contextRegistration.RegisterFunctions(Decorate<ActionMenuGenerationContext>(_rootScriptObject));
        }

        PushGlobal(_rootScriptObject);

        // -2 because _rootScriptObject and RepositoryActionsScriptContext are already added
        if (@this.GlobalCount -2 != @this._globals.Count)
        {
            throw new Exception();
        }

        _repositoryActionsScriptContext = new DisposableContextScriptObject(this, _contextActionMappers);
        PushGlobal(_repositoryActionsScriptContext);

        for (var index = 0; index < @this._globals.Count; index++)
        {
            @this._globals.Items[index].CloneUsingNewContext(this);
        }
    }

    private RepoMScriptObject CreateAndInitRepoMScriptObject(EnvSetScriptObject env)
    {
        var scriptObj = new RepoMScriptObject();

        scriptObj.Import(typeof(InitialFunctions));

        scriptObj.SetValue("file", new FileFunctions(), true);
        scriptObj.SetValue("repository", new RepositoryFunctions(Repository), true);

        scriptObj.Add("env", env);
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
        await foreach (UserInterfaceRepositoryActionBase item in mapper.MapAsync(in menuAction, this, Repository).ConfigureAwait(false))
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
        return new DisposableContextScriptObject(this, _contextActionMappers);
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