namespace RepoM.ActionMenu.Core;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using RepoM.ActionMenu.Core.ConfigReader;
using RepoM.ActionMenu.Core.Misc;
using RepoM.ActionMenu.Core.Model;
using RepoM.ActionMenu.Core.Services;
using RepoM.ActionMenu.Core.Yaml.Serialization;
using RepoM.ActionMenu.Interface.Scriban;
using RepoM.ActionMenu.Interface.YamlModel;
using RepoM.Core.Plugin.RepositoryOrdering.Configuration;
using SimpleInjector;

public static class Bootstrapper
{
    private static readonly Assembly _thisAssembly = typeof(Bootstrapper).Assembly;

    public static void RegisterServices(Container container)
    {
        ArgumentNullException.ThrowIfNull(container);
        RegisterPrivateTypes(container);
        RegisterPublicTypes(container);
    }

    private static void RegisterPublicTypes(Container container)
    {
        container.Register<IUserInterfaceActionMenuFactory, UserInterfaceActionMenuFactory>(Lifestyle.Singleton);
        container.RegisterDecorator<IUserInterfaceActionMenuFactory, UserInterfaceActionMenuFactoryTaskDecorator>(Lifestyle.Singleton);
    }

    private static void RegisterPrivateTypes(Container container)
    {
        IEnumerable<Type> assemblyExportableTypes = GetExportedTypesFrom(_thisAssembly).ToArray();

        container.Collection.Register<ITemplateContextRegistration>(Array.Empty<Type>(), Lifestyle.Singleton);

        IEnumerable<Type> types = assemblyExportableTypes
          .Where(t => typeof(IActionToRepositoryActionMapper).GetTypeInfo().IsAssignableFrom(t.GetTypeInfo()))
          .Where(t => t.GetTypeInfo() is { IsAbstract: false, IsGenericTypeDefinition: false, });
        container.Collection.Register<IActionToRepositoryActionMapper>(types, Lifestyle.Singleton);

        types = assemblyExportableTypes
            .Where(t => typeof(IMenuAction).GetTypeInfo().IsAssignableFrom(t.GetTypeInfo()))
            .Where(t => t.GetTypeInfo() is { IsAbstract: false, IsGenericTypeDefinition: false, });
        foreach (Type type in types)
        {
            container.RegisterActionMenuType(type);
        }

        container.RegisterSingleton<ITemplateParser, FixedTemplateParser>();
        container.RegisterSingleton<IActionMenuDeserializer, ActionMenuDeserializer>();

        container.RegisterSingleton<IFileReader, FileReader>();
        container.RegisterDecorator<IFileReader, CacheFileReaderDecorator>(Lifestyle.Singleton);
    }

    /// <summary>
    /// The one and only public interface for this module.
    /// </summary>
    /// <param name="container">The container</param>
    /// <returns>Instance of <see cref="IUserInterfaceActionMenuFactory"/>.</returns>
    public static IUserInterfaceActionMenuFactory GetUserInterfaceActionMenu(Container container)
    {
        return container.GetInstance<IUserInterfaceActionMenuFactory>();
    }

    private static void RegisterActionMenuType(this Container container, Type type)
    {
        container.Collection.AppendInstance<IKeyTypeRegistration<IMenuAction>>(new FixedTypeRegistration<IMenuAction>(type, TypeRepositoryActionAttributeReader.GetValue(type)));
    }

    static IEnumerable<Type> GetExportedTypesFrom(Assembly assembly)
    {
        try
        {
            return assembly.DefinedTypes.Select(info => info.AsType());
        }
        catch (NotSupportedException)
        {
            // A type load exception would typically happen on an Anonymously Hosted DynamicMethods
            // Assembly and it would be safe to skip this exception.
            return Enumerable.Empty<Type>();
        }
    }
}

[DebuggerDisplay($"{{{nameof(Tag)}}}")]
file sealed class FixedTypeRegistration<T> : IKeyTypeRegistration<T>
{
    public FixedTypeRegistration(Type configurationType, string tag)
    {
        ConfigurationType = configurationType;

        if (string.IsNullOrEmpty(tag))
        {
            throw new ArgumentNullException(nameof(tag));
        }

        Tag = tag;
    }

    public Type ConfigurationType { get; }

    public string Tag { get; }
}

/// <summary>
/// This class 'assumes' that the type has a custom attribute of type <see cref="RepositoryActionAttribute"/> with a property 'Type' that is the type value.
/// </summary>
file static class TypeRepositoryActionAttributeReader
{
    public static string GetValue(Type type)
    {
        return type.GetCustomAttribute<RepositoryActionAttribute>()?.Type ?? throw new InvalidOperationException($"RepositoryActionAttribute not found on {type.FullName}");
    }
}