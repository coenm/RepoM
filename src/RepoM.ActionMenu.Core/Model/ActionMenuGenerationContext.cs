namespace RepoM.ActionMenu.Core.Model;

using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using System.Threading.Tasks;
using RepoM.ActionMenu.Core.ActionMenu.Context;
using RepoM.ActionMenu.Core.ConfigReader;
using RepoM.ActionMenu.Core.Misc;
using RepoM.ActionMenu.Core.Yaml.Model.ActionContext;
using RepoM.ActionMenu.Core.Yaml.Model.ActionContext.EvaluateVariable;
using RepoM.ActionMenu.Core.Yaml.Model.ActionContext.ExecuteScript;
using RepoM.ActionMenu.Core.Yaml.Model.ActionContext.LoadFile;
using RepoM.ActionMenu.Core.Yaml.Model.ActionContext.RendererVariable;
using RepoM.ActionMenu.Core.Yaml.Model.ActionContext.SetVariable;
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
using static RepoM.ActionMenu.Core.Model.ScribanModuleWithFunctions;

internal class ActionMenuGenerationContext : TemplateContext, IActionMenuGenerationContext, IContextMenuActionMenuGenerationContext
{
    private readonly ITemplateParser _templateParser;
    private readonly ITemplateContextRegistration[] _functionsArray;
    private readonly IActionMenuDeserializer _deserializer;
    private readonly IFileReader _fileReader;
    private readonly IActionToRepositoryActionMapper[] _repositoryActionMappers;
    private readonly List<IContextActionProcessor> _contextActionMappers;
    private readonly RepoMScriptObject _rootScriptObject; // used for cloning.

    public ActionMenuGenerationContext(
        IRepository repository, // runtime data, todo
        ITemplateParser templateParser, 
        IFileSystem fileSystem,
        ITemplateContextRegistration[] functionsArray,
        IActionToRepositoryActionMapper[] repositoryActionMappers,
        IActionMenuDeserializer deserializer,
        IFileReader fileReader)
    {
        _templateParser = templateParser ?? throw new ArgumentNullException(nameof(templateParser));
        _functionsArray = functionsArray ?? throw new ArgumentNullException(nameof(functionsArray));
        FileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
        Repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _repositoryActionMappers = repositoryActionMappers ?? throw new ArgumentNullException(nameof(repositoryActionMappers));
        _deserializer = deserializer ?? throw new ArgumentNullException(nameof(deserializer));
        _fileReader = fileReader ?? throw new ArgumentNullException(nameof(fileReader));

        _contextActionMappers = new List<IContextActionProcessor>
            {
                new ContextActionExecuteScriptV1Processor(),
                new ContextActionSetVariableV1Processor(),
                new ContextActionEvaluateVariableV1Processor(),
                new ContextActionRenderVariableV1Processor(),
                new ContextActionLoadFileV1Processor(_fileReader),
            };

        _rootScriptObject = CreateAndInit(Repository);

       
        foreach (ITemplateContextRegistration contextRegistration in _functionsArray)
        {
            contextRegistration.RegisterFunctions(Decorate<ActionMenuGenerationContext>(_rootScriptObject));
        }
        
        PushGlobal(_rootScriptObject);
        RepositoryActionsScriptContext = new DisposableContextScriptObject(this, Env, _contextActionMappers);
        PushGlobal(RepositoryActionsScriptContext);
    }

    private static RepoMScriptObject CreateAndInit(IRepository repository)
    {
        var scriptObj = new RepoMScriptObject();

        scriptObj.SetValue("file", new FileFunctions(), true);
        scriptObj.SetValue("repository", new RepositoryFunctions(repository), true);

        scriptObj.Add("env", new EnvSetScriptObject(EnvScriptObject.Create()));
        scriptObj.SetReadOnly("env", false); // this is not what we want, but it's the only way to make it work

        return scriptObj;
    }

    private ActionMenuGenerationContext(
        IRepository repository, // runtime data, todo
        ITemplateParser templateParser,
        IFileSystem fileSystem,
        ITemplateContextRegistration[] functionsArray,
        IActionToRepositoryActionMapper[] repositoryActionMappers,
        IActionMenuDeserializer deserializer,
        IFileReader fileReader,

        RepoMScriptObject rootScriptObject // is it required to be an RepoM... object?
    )
    {
        _templateParser = templateParser ?? throw new ArgumentNullException(nameof(templateParser));
        _functionsArray = functionsArray ?? throw new ArgumentNullException(nameof(functionsArray));
        FileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
        Repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _repositoryActionMappers = repositoryActionMappers ?? throw new ArgumentNullException(nameof(repositoryActionMappers));
        _deserializer = deserializer ?? throw new ArgumentNullException(nameof(deserializer));
        _fileReader = fileReader ?? throw new ArgumentNullException(nameof(fileReader));

        _contextActionMappers = new List<IContextActionProcessor>
            {
                new ContextActionExecuteScriptV1Processor(),
                new ContextActionSetVariableV1Processor(),
                new ContextActionEvaluateVariableV1Processor(),
                new ContextActionRenderVariableV1Processor(),
                new ContextActionLoadFileV1Processor(_fileReader),
            };

        _rootScriptObject = rootScriptObject ?? throw new ArgumentNullException(nameof(rootScriptObject));

        PushGlobal(_rootScriptObject);
        RepositoryActionsScriptContext = new DisposableContextScriptObject(this, Env, _contextActionMappers);
        PushGlobal(RepositoryActionsScriptContext);
    }

    private static IContextRegistration Decorate<T>(IContextRegistration rootScriptObject) where T : TemplateContext
    {
        return new ContextRegistrationDecorator<T>(rootScriptObject);
    }

    public IFileSystem FileSystem { get; }

    public DisposableContextScriptObject RepositoryActionsScriptContext { get; }

    public IRepository Repository { get; }

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

        var repoMScriptObject = (RepoMScriptObject)_rootScriptObject.Clone(true);

        IScriptObject e = ((EnvSetScriptObject)_rootScriptObject["env"]).Clone(true);
        repoMScriptObject.SetValue("env", e, false);

        var result = new ActionMenuGenerationContext(
            Repository,
            _templateParser,
            FileSystem,
            _functionsArray,
            _repositoryActionMappers,
            _deserializer,
            _fileReader,

            repoMScriptObject);

        return result;
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
}

internal class ContextRegistrationDecorator<T> : IContextRegistration where T : TemplateContext
{
    private readonly IContextRegistration _contextRegistration;

    public ContextRegistrationDecorator(IContextRegistration contextRegistration)
    {
        _contextRegistration = contextRegistration ?? throw new ArgumentNullException(nameof(contextRegistration));
    }

    public IContextRegistration CreateOrGetSubRegistration(string key)
    {
        return new ContextRegistrationDecorator<T>(_contextRegistration.CreateOrGetSubRegistration(key));
    }

    public void SetValue(string member, object value, bool readOnly)
    {
        _contextRegistration.SetValue(member, value, readOnly);
    }

    public void Add(string key, object value)
    {
        _contextRegistration.Add(key, value);
    }

    public bool ContainsKey(string key)
    {
        return _contextRegistration.ContainsKey(key);
    }

    public void RegisterConstant(string name, object value)
    {
        _contextRegistration.RegisterConstant(name, value);
    }

    public void RegisterAction(string name, Action action)
    {
        _contextRegistration.RegisterAction(name, action);
    }

    public void RegisterAction<T1>(string name, Action<T1> action)
    {
        if (Check<T1>())
        {
            DelegateCustomFunction value = new InternalDelegateCustomActionWithInterfaceContext<T, T1>(action);
            _contextRegistration.RegisterVariable(name, value);
            return;
        }

        _contextRegistration.RegisterAction(name, action);
    }

    public void RegisterAction<T1, T2>(string name, Action<T1, T2> action)
    {
        if (Check<T1>())
        {
            DelegateCustomFunction value = new InternalDelegateCustomActionWithInterfaceContext<T, T1, T2>(action);
            _contextRegistration.RegisterVariable(name, value);
            return;
        }

        _contextRegistration.RegisterAction(name, action);
    }

    public void RegisterAction<T1, T2, T3>(string name, Action<T1, T2, T3> action)
    {
        if (Check<T1>())
        {
            DelegateCustomFunction value = new InternalDelegateCustomActionWithInterfaceContext<T, T1, T2, T3>(action);
            _contextRegistration.RegisterVariable(name, value);
            return;
        }

        _contextRegistration.RegisterAction(name, action);
    }

    public void RegisterAction<T1, T2, T3, T4>(string name, Action<T1, T2, T3, T4> action)
    {
        if (Check<T1>())
        {
            DelegateCustomFunction value = new InternalDelegateCustomActionWithInterfaceContext<T, T1, T2, T3, T4>(action);
            _contextRegistration.RegisterVariable(name, value);
            return;
        }

        _contextRegistration.RegisterAction(name, action);
    }

    public void RegisterAction<T1, T2, T3, T4, T5>(string name, Action<T1, T2, T3, T4, T5> action)
    {
        if (Check<T1>())
        {
            DelegateCustomFunction value = new InternalDelegateCustomActionWithInterfaceContext<T, T1, T2, T3, T4, T5>(action);
            _contextRegistration.RegisterVariable(name, value);
            return;
        }

        _contextRegistration.RegisterAction(name, action);
    }

    public void RegisterFunction<T1>(string name, Func<T1> func)
    {
        _contextRegistration.RegisterFunction(name, func);
    }

    public void RegisterFunction<T1, T2>(string name, Func<T1, T2> func)
    {
        if (Check<T1>())
        {
            DelegateCustomFunction value = new InternalDelegateCustomFunctionWithInterfaceContext<T, T1, T2>(func);
            _contextRegistration.RegisterVariable(name, value);
            return;
        }

        _contextRegistration.RegisterFunction(name, func);
    }

    public void RegisterFunction<T1, T2, T3>(string name, Func<T1, T2, T3> func)
    {
        if (Check<T1>())
        {
            DelegateCustomFunction value = new InternalDelegateCustomFunctionWithInterfaceContext<T, T1, T2, T3>(func);
            _contextRegistration.RegisterVariable(name, value);
            return;
        }

        _contextRegistration.RegisterFunction(name, func);
    }

    public void RegisterFunction<T1, T2, T3, T4>(string name, Func<T1, T2, T3, T4> func)
    {
        if (Check<T1>())
        {
            DelegateCustomFunction value = new InternalDelegateCustomFunctionWithInterfaceContext<T, T1, T2, T3, T4>(func);
            _contextRegistration.RegisterVariable(name, value);
            return;
        }

        _contextRegistration.RegisterFunction(name, func);
    }

    public void RegisterFunction<T1, T2, T3, T4, T5>(string name, Func<T1, T2, T3, T4, T5> func)
    {
        if (Check<T1>())
        {
            DelegateCustomFunction value = new InternalDelegateCustomFunctionWithInterfaceContext<T, T1, T2, T3, T4, T5>(func);
            _contextRegistration.RegisterVariable(name, value);
            return;
        }
        
        _contextRegistration.RegisterFunction(name, func);
    }

    public void RegisterFunction<T1, T2, T3, T4, T5, T6>(string name, Func<T1, T2, T3, T4, T5, T6> func)
    {
        if (Check<T1>())
        {
            DelegateCustomFunction value = new InternalDelegateCustomFunctionWithInterfaceContext<T, T1, T2, T3, T4, T5, T6>(func);
            _contextRegistration.RegisterVariable(name, value);
            return;
        }

        _contextRegistration.RegisterFunction(name, func);
    }

    public void RegisterVariable(string name, object value)
    {
        _contextRegistration.RegisterVariable(name, value);
    }

    private static bool Check<T1>()
    {
        // todo check.
        var isInterface = typeof(T1).IsInterface;
        var isAssignableFrom = typeof(T1).IsAssignableFrom(typeof(T));
        return isInterface && isAssignableFrom;
    }
}