namespace RepoM.ActionMenu.Core.Yaml.Serialization;

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
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

internal class ActionMenuDeserializer : IActionMenuDeserializer
{
    private readonly IDeserializer _deserializer;
    private readonly ISerializer _serializer;

    private static readonly Dictionary<Type, Func<object>> _factoryMethods = new()
        {
            { typeof(Script), () => new ScribanScript() },
            { typeof(Variable), () => new ScribanVariable() },
            { typeof(Predicate), () => new ScribanPredicate() },
            { typeof(Text), () => new ScribanText() },
        };
    private readonly ILogger _logger;

    public ActionMenuDeserializer(IEnumerable<IKeyTypeRegistration<IMenuAction>> keyTypeRegistrations, ITemplateParser templateParser, ILogger logger)
        : this(keyTypeRegistrations.ToDictionary(item => item.Tag, item => item.ConfigurationType), templateParser, logger)
    {
    }

    private ActionMenuDeserializer(IDictionary<string, Type> menuActionTypes, ITemplateParser templateParser, ILogger logger)
    {
        ArgumentNullException.ThrowIfNull(menuActionTypes);
        ArgumentNullException.ThrowIfNull(templateParser);
        ArgumentNullException.ThrowIfNull(logger);

        _logger = logger;

        _serializer = new SerializerBuilder()
            .WithNamingConvention(HyphenatedNamingConvention.Instance) // CamelCaseNamingConvention.Instance
            .WithDefaultScalarStyle(ScalarStyle.Any)
            .WithTypeConverter(new EvaluateObjectConverter(_factoryMethods))
            .Build();

        _deserializer = new DeserializerBuilder()
            .WithNamingConvention(HyphenatedNamingConvention.Instance)
            .WithTypeConverter(new EvaluateObjectConverter(_factoryMethods))
            .WithTypeConverter(new DefaultContextActionTypeConverter())
            .WithTypeDiscriminatingNodeDeserializer(options =>
                {
                    options.AddTypeDiscriminator(
                        new KeyValueTypeDiscriminatorWithDefaultType<IContextAction, DefaultV1Type>(
                            "type",
                            new Dictionary<string, Type>
                               {
                                    // This is okay as the context types are not extendable at the moment.
                                    // Otherwise, inject like IMenuAction.`
                                    { ContextActionEvaluateVariableV1.TYPE_VALUE, typeof(ContextActionEvaluateVariableV1)},
                                    { ContextActionRenderVariableV1.TYPE_VALUE, typeof(ContextActionRenderVariableV1)},
                                    { ContextActionExecuteScriptV1.TYPE_VALUE, typeof(ContextActionExecuteScriptV1)},
                                    { ContextActionSetVariableV1.TYPE_VALUE, typeof(ContextActionSetVariableV1)},
                                    { ContextActionLoadFileV1.TYPE_VALUE, typeof(ContextActionLoadFileV1)},
                                }));
                    options.AddTypeDiscriminator(new RequiredKeyValueTypeDiscriminator<IMenuAction>("type", menuActionTypes, _logger));
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

    public T Deserialize<T>(string content) where T : ContextRoot
    {
        try
        {
            return _deserializer.Deserialize<T>(content);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }   

    public string Serialize<T>(T actionMenuRoot) where T : ContextRoot
    {
        return _serializer.Serialize(actionMenuRoot, typeof(T));
    }
}