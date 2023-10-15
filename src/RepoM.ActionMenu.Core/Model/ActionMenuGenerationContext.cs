namespace RepoM.ActionMenu.Core.Model;

using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using System.Threading.Tasks;
using RepoM.ActionMenu.Core.Misc;
using RepoM.ActionMenu.Core.Model.Env;
using RepoM.ActionMenu.Core.Model.Functions;
using RepoM.ActionMenu.Core.Yaml.Model;
using RepoM.ActionMenu.Core.Yaml.Model.ActionContext;
using RepoM.ActionMenu.Core.Yaml.Model.ActionContext.EvaluateVariable;
using RepoM.ActionMenu.Core.Yaml.Model.ActionContext.ExecuteScript;
using RepoM.ActionMenu.Core.Yaml.Model.ActionContext.LoadFile;
using RepoM.ActionMenu.Core.Yaml.Model.ActionContext.RendererVariable;
using RepoM.ActionMenu.Core.Yaml.Model.ActionContext.SetVariable;
using RepoM.ActionMenu.Core.Yaml.Model.ActionMenus;
using RepoM.ActionMenu.Core.Yaml.Model.ActionMenus.AssociateFile;
using RepoM.ActionMenu.Core.Yaml.Model.ActionMenus.BrowseRepository;
using RepoM.ActionMenu.Core.Yaml.Model.ActionMenus.Command;
using RepoM.ActionMenu.Core.Yaml.Model.ActionMenus.Folder;
using RepoM.ActionMenu.Core.Yaml.Model.ActionMenus.ForEach;
using RepoM.ActionMenu.Core.Yaml.Model.ActionMenus.Git.Checkout;
using RepoM.ActionMenu.Core.Yaml.Model.ActionMenus.JustText;
using RepoM.ActionMenu.Core.Yaml.Model.Tags;
using RepoM.ActionMenu.Core.Yaml.Serialization;
using RepoM.ActionMenu.Interface.ActionMenuFactory;
using RepoM.ActionMenu.Interface.Scriban;
using RepoM.ActionMenu.Interface.UserInterface;
using RepoM.ActionMenu.Interface.YamlModel;
using RepoM.Core.Plugin.Repository;
using Scriban;

internal class ActionMenuGenerationContext : TemplateContext, IActionMenuGenerationContext, IContextMenuActionMenuGenerationContext
{
    private readonly ITemplateParser _templateParser;
    private readonly ITemplateContextRegistration[] _functionsArray;
    private readonly ActionMenuDeserializer _deserializer = new();
    private readonly List<IActionToRepositoryActionMapper> _repositoryActionMappers;
    private readonly List<IContextActionProcessor> _contextActionMappers;

    public ActionMenuGenerationContext(
        IRepository repository, 
        ITemplateParser templateParser, 
        IFileSystem fileSystem,
        ITemplateContextRegistration[] functionsArray)
    {
        _templateParser = templateParser ?? throw new ArgumentNullException(nameof(templateParser));
        _functionsArray = functionsArray ?? throw new ArgumentNullException(nameof(functionsArray));
        FileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
        Repository = repository ?? throw new ArgumentNullException(nameof(repository));

        _repositoryActionMappers = new List<IActionToRepositoryActionMapper>()
            {
                new RepositoryActionAssociateFileV1Mapper(),
                new RepositoryActionJustTextV1Mapper(),
                new RepositoryActionFolderV1Mapper(),
                new RepositoryActionBrowseRepositoryV1Mapper(),
                new ActionCommandV1Mapper(),
                new RepositoryActionForEachV1Mapper(),
                new RepositoryActionGitCheckoutV1Mapper(),
            };

        var rootScriptObject = new RepoMScriptObject();
        
        rootScriptObject.SetValue("file", new FileFunctions(), true);


        rootScriptObject.Add("repository", new RepositoryFunctions(Repository));
        rootScriptObject.SetReadOnly("repository", true);

        Env = new EnvSetScriptObject(EnvScriptObject.Create());
        rootScriptObject.Add("env", Env);
        rootScriptObject.SetReadOnly("env", true);

        foreach (var x in _functionsArray)
        {
            x.RegisterFunctionsAuto(rootScriptObject);
        }


        PushGlobal(rootScriptObject);

        _contextActionMappers = new List<IContextActionProcessor>
            {
                new ContextActionExecuteScriptV1Processor(),
                new ContextActionSetVariableV1Processor(),
                new ContextActionEvaluateVariableV1Processor(),
                new ContextActionRenderVariableV1Processor(),
                new ContextActionLoadFileV1Processor(FileSystem, _deserializer),
            };

        RepositoryActionsScriptContext = new DisposableContextScriptObject(this, Env, _contextActionMappers);
        PushGlobal(RepositoryActionsScriptContext);
    }

    public IFileSystem FileSystem { private set; get; }

    public DisposableContextScriptObject RepositoryActionsScriptContext { get; private set; }

    public IRepository Repository { get; }

    public EnvSetScriptObject Env { get; private set; }
    
    public async Task AddRepositoryContextAsync(Context? reposContext)
    {
        if (reposContext == null)
        {
            return;
        }

        foreach (var contextAction in reposContext)
        {
            await RepositoryActionsScriptContext.AddContextActionAsync(contextAction).ConfigureAwait(false);
        }
    }

    public async Task<IEnumerable<UserInterfaceRepositoryActionBase>> AddActionMenusAsync(List<IMenuAction>? menus)
    {
        if (menus == null)
        {
            return Array.Empty<UserInterfaceRepositoryActionBase>();
        }

        var items = new List<UserInterfaceRepositoryActionBase>();

        using var disposable = CreateGlobalScope();

        foreach (var action in menus)
        {
            foreach (var item in await AddMenuActionAsync(action).ConfigureAwait(false))
            {
                items.Add(item);
            }
        }

        return items;
    }

    public IActionMenuGenerationContext Clone()
    {
        throw new NotImplementedException("Not a full clone.");
        var result = new ActionMenuGenerationContext(Repository, _templateParser, FileSystem, _functionsArray)
        {
            Env = (EnvSetScriptObject)Env.Clone(true),
            RepositoryActionsScriptContext = (DisposableContextScriptObject)RepositoryActionsScriptContext.Clone(true)
        };
        // todo more
        return result;
    }

    private async Task<IEnumerable<UserInterfaceRepositoryActionBase>> AddMenuActionAsync(IMenuAction menuAction)
    {
        if (!await IsMenuItemActiveAsync(menuAction))
        {
            return Array.Empty<UserInterfaceRepositoryActionBase>();
        }

        using var variableContext = PushNewContext();

        if (menuAction is IContext { Context: not null } c)
        {
            foreach (var ctx in c.Context)
            {
                await variableContext.AddContextActionAsync(ctx).ConfigureAwait(false);
            }
        }

        var mapper = _repositoryActionMappers.Find(mapper => mapper.CanMap(menuAction));
        if (mapper == null)
        {
            // throw?
            return Array.Empty<UserInterfaceRepositoryActionBase>();
        }

        var items = new List<UserInterfaceRepositoryActionBase>();
        await foreach (var item in mapper.MapAsync(menuAction, this, Repository).ConfigureAwait(false))
        {
            items.Add(item);
        }

        return items;
    }

    public async Task<string> RenderStringAsync(string text)
    {
        var template = _templateParser.ParseMixed(text);
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

        var template = _templateParser.ParseScriptOnly(text);
        return await template.EvaluateAsync(this).ConfigureAwait(false);
    }
    
    private Task<bool> IsMenuItemActiveAsync(IMenuAction menuAction)
    {
        return this.EvaluateToBooleanAsync(menuAction.Active, true);
    }

    public IScope CreateGlobalScope()
    {
        return PushNewContext();
    }

    internal async Task<Root> LoadAsync(string filename)
    {
        var yaml = await FileSystem.File.ReadAllTextAsync(filename).ConfigureAwait(false);
        var actions = _deserializer.DeserializeRoot(yaml);
        return actions ?? throw new NotImplementedException("Could not deserialize file");
    }

    public async Task<IEnumerable<string>> GetTagsAsync(Tags taqs)
    {
        if (taqs == null)
        {
            return Array.Empty<string>();
        }

        var items = new List<string>(Tags.Count);

        using var disposable = CreateGlobalScope();

        foreach (var tag in taqs)
        {
            if (await tag.When.EvaluateAsync(this).ConfigureAwait(false))
            {
                items.Add(tag.Tag);
            }
        }

        return items.Distinct();
    }
}