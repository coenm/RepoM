namespace RepoM.ActionMenu.Core.Yaml.Serialization;

using System;
using System.Collections.Generic;
using System.Linq;
using RepoM.ActionMenu.Core.Misc;
using RepoM.ActionMenu.Core.Model;
using RepoM.ActionMenu.Core.Yaml.Model;
using RepoM.ActionMenu.Core.Yaml.Model.ActionContext.EvaluateVariable;
using RepoM.ActionMenu.Core.Yaml.Model.ActionContext.ExecuteScript;
using RepoM.ActionMenu.Core.Yaml.Model.ActionContext.LoadFile;
using RepoM.ActionMenu.Core.Yaml.Model.ActionContext.RendererVariable;
using RepoM.ActionMenu.Core.Yaml.Model.ActionContext.SetVariable;
using RepoM.ActionMenu.Core.Yaml.Model.Tags;
using RepoM.ActionMenu.Core.Yaml.Model.Templating;
using RepoM.ActionMenu.Interface.YamlModel;
using RepoM.ActionMenu.Interface.YamlModel.Templating;
using RepoM.Core.Plugin.RepositoryOrdering.Configuration;
using YamlDotNet.Core;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.BufferedDeserialization;
using YamlDotNet.Serialization.NamingConventions;
using YamlDotNet.Serialization.NodeDeserializers;
using YamlDotNet.Serialization.TypeResolvers;

internal class ActionMenuDeserializer : IActionMenuDeserializer
{
    private readonly IDeserializer _deserializer;
    private readonly ISerializer _serializer;

    public ActionMenuDeserializer(IEnumerable<IKeyTypeRegistration<IMenuAction>> keyTypeRegistrations, ITemplateParser templateParser)
        : this(keyTypeRegistrations.ToDictionary(item => item.Tag, item => item.ConfigurationType), templateParser)
    {
    }

    private ActionMenuDeserializer(IDictionary<string, Type> menuActionTypes, ITemplateParser templateParser)
    {
        if (menuActionTypes == null)
        {
            throw new ArgumentNullException(nameof(menuActionTypes));
        }

        if (templateParser == null)
        {
            throw new ArgumentNullException(nameof(templateParser));
        }

        var factoryMethods = new Dictionary<Type, Func<object>>
            {
                { typeof(Script), () => new ScribanScript() },
                { typeof(Variable), () => new ScribanVariable() },
                { typeof(Predicate), () => new ScribanPredicate() },
                { typeof(Text), () => new ScribanText() },
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
                        new KeyValueTypeDiscriminatorWithDefaultType<IContextAction, DefaultV1Type>(
                            "type",
                            new Dictionary<string, Type>
                               {
                                    // This is okay as the context types are not extendable at the moment.
                                    // Otherwise; inject like IMenuAction.`
                                    { ContextActionEvaluateVariableV1.TYPE_VALUE, typeof(ContextActionEvaluateVariableV1)},
                                    { ContextActionRenderVariableV1.TYPE_VALUE, typeof(ContextActionRenderVariableV1)},
                                    { ContextActionExecuteScriptV1.TYPE_VALUE, typeof(ContextActionExecuteScriptV1)},
                                    { ContextActionSetVariableV1.TYPE_VALUE, typeof(ContextActionSetVariableV1)},
                                    { ContextActionLoadFileV1.TYPE_VALUE, typeof(ContextActionLoadFileV1)},
                                }));
                    options.AddKeyValueTypeDiscriminator<IMenuAction>("type", menuActionTypes);
                },
                maxDepth: -1,
                maxLength: -1)
            .WithTypeMapping<ITag, TagObject>()
            .WithNodeDeserializer(inner => new TemplateUpdatingNodeDeserializer<ObjectNodeDeserializer>(inner, templateParser), s => s.InsteadOf<ObjectNodeDeserializer>())
            .WithNodeDeserializer(inner => new TemplateUpdatingNodeDeserializer<ScalarNodeDeserializer>(inner, templateParser), s => s.InsteadOf<ScalarNodeDeserializer>())
            .WithNodeDeserializer(inner => new TemplateUpdatingNodeDeserializer<TypeDiscriminatingNodeDeserializer>(inner, templateParser), s => s.InsteadOf<TypeDiscriminatingNodeDeserializer>())
            .WithNodeDeserializer(inner => new TemplateUpdatingNodeDeserializer<TypeConverterNodeDeserializer>(inner, templateParser), s => s.InsteadOf<TypeConverterNodeDeserializer>())
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