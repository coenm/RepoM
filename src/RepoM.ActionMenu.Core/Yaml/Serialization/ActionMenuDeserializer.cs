namespace RepoM.ActionMenu.Core.Yaml.Serialization;

using System;
using System.Collections.Generic;
using RepoM.ActionMenu.Core.Model;
using RepoM.ActionMenu.Core.Yaml.Model;
using RepoM.ActionMenu.Core.Yaml.Model.ActionMenus.AssociateFile;
using RepoM.ActionMenu.Core.Yaml.Model.ActionMenus.BrowseRepository;
using RepoM.ActionMenu.Core.Yaml.Model.ActionMenus.Command;
using RepoM.ActionMenu.Core.Yaml.Model.ActionMenus.Folder;
using RepoM.ActionMenu.Core.Yaml.Model.ActionMenus.ForEach;
using RepoM.ActionMenu.Core.Yaml.Model.ActionMenus.JustText;
using RepoM.ActionMenu.Core.Yaml.Model.Ctx.EvaluateVariable;
using RepoM.ActionMenu.Core.Yaml.Model.Ctx.ExecuteScript;
using RepoM.ActionMenu.Core.Yaml.Model.Ctx.LoadFile;
using RepoM.ActionMenu.Core.Yaml.Model.Ctx.RendererVariable;
using RepoM.ActionMenu.Core.Yaml.Model.Ctx.SetVariable;
using RepoM.ActionMenu.Core.Yaml.Model.Tags;
using RepoM.ActionMenu.Core.Yaml.Model.Templating;
using RepoM.ActionMenu.Interface.YamlModel;
using RepoM.ActionMenu.Interface.YamlModel.Templating;
using YamlDotNet.Core;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.BufferedDeserialization;
using YamlDotNet.Serialization.NamingConventions;
using YamlDotNet.Serialization.NodeDeserializers;

internal class ActionMenuDeserializer : IActionMenuDeserializer
{
    private readonly IDeserializer _deserializer;
    private readonly ISerializer _serializer;
    
    public ActionMenuDeserializer()
    {
        var factoryMethods = new Dictionary<Type, Func<object>>
            {
                { typeof(EvaluateBoolean), () => new ScribanEvaluateBoolean() },
                { typeof(RenderString), () => new ScribanRenderString() },
                { typeof(EvaluateInt), () => new ScribanEvaluateInt() },
            };

        _serializer = new SerializerBuilder()
            .WithNamingConvention(HyphenatedNamingConvention.Instance) // CamelCaseNamingConvention.Instance
            .WithDefaultScalarStyle(ScalarStyle.Any)
            .WithTypeConverter(new EvaluateObjectConverter(factoryMethods))
            .Build();

        _deserializer = new DeserializerBuilder()
            .WithNamingConvention(HyphenatedNamingConvention.Instance)
            .WithTypeConverter(new EvaluateObjectConverter(factoryMethods))
            .WithTypeConverter(new DefaultContextActionTypeConverter())
            .WithTypeDiscriminatingNodeDeserializer(options =>
                {
                    options.AddTypeDiscriminator(
                        new KeyValueTypeDiscriminatorWithDefaultType<IContextAction, DefaultContextActionType>(
                            "type",
                            new Dictionary<string, Type>
                               {
                                    { EvaluateVariableContextAction.TypeValue, typeof(EvaluateVariableContextAction)},
                                    { RenderVariableContextAction.TypeValue, typeof(RenderVariableContextAction)},
                                    { ExecuteScript.TypeValue, typeof(ExecuteScript)},
                                    { SetVariableContextAction.TypeValue, typeof(SetVariableContextAction)},
                                    { LoadFileContextAction.TypeValue, typeof(LoadFileContextAction)},
                                }));

                    options.AddKeyValueTypeDiscriminator<IMenuAction>(
                        "type",
                        new Dictionary<string, Type>
                            {
                                { RepositoryActionAssociateFileV1.TypeValue, typeof(RepositoryActionAssociateFileV1) },
                                { RepositoryActionJustTextV1.TypeValue, typeof(RepositoryActionJustTextV1) },
                                { RepositoryActionFolderV1.TypeValue, typeof(RepositoryActionFolderV1) },
                                { RepositoryActionBrowseRepositoryV1.TypeValue, typeof(RepositoryActionBrowseRepositoryV1) },
                                { RepositoryActionCommandV1.TypeValue, typeof(RepositoryActionCommandV1) },
                                { RepositoryActionForEachV1.TypeValue, typeof(RepositoryActionForEachV1) },
                            });
                },
                maxDepth: -1,
                maxLength: -1)
            .WithTypeMapping<ITag, TagObject>()
            .WithNodeDeserializer(inner => new TemplateUpdatingNodeDeserializer<ObjectNodeDeserializer>(inner), s => s.InsteadOf<ObjectNodeDeserializer>())
            .WithNodeDeserializer(inner => new TemplateUpdatingNodeDeserializer<ScalarNodeDeserializer>(inner), s => s.InsteadOf<ScalarNodeDeserializer>())
            .WithNodeDeserializer(inner => new TemplateUpdatingNodeDeserializer<TypeDiscriminatingNodeDeserializer>(inner), s => s.InsteadOf<TypeDiscriminatingNodeDeserializer>())
            .WithNodeDeserializer(inner => new TemplateUpdatingNodeDeserializer<TypeConverterNodeDeserializer>(inner), s => s.InsteadOf<TypeConverterNodeDeserializer>())
            .Build();
    }

    public Root DeserializeRoot(string content)
    {
        try
        {
            return _deserializer.Deserialize<Root>(content);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public ContextRoot DeserializeContextRoot(string content)
    {
        try
        {
            return _deserializer.Deserialize<ContextRoot>(content);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public string Serialize(Root root)
    {
        return _serializer.Serialize(root, typeof(Root));
    }
}