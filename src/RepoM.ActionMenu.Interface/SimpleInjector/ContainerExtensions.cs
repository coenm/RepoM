namespace RepoM.ActionMenu.Interface.SimpleInjector;

using RepoM.ActionMenu.Interface.YamlModel;
using RepoM.Core.Plugin.RepositoryOrdering.Configuration;
using System;
using System.Reflection;
using global::SimpleInjector;

public static class ContainerExtensions
{
    public static void RegisterActionMenuMapper<T>(this Container container, Lifestyle lifestyle) where T : class, IActionToRepositoryActionMapper
    {
        container.Collection.Append<IActionToRepositoryActionMapper, T>(lifestyle);
    }

    public static void RegisterActionMenuType<T>(this Container container) where T : IMenuAction
    {
        RegisterActionMenuType(container, typeof(T));
    }

    public static void RegisterActionMenuType(this Container container, Type type)
    {
        container.Collection.AppendInstance<IKeyTypeRegistration<IMenuAction>>(new FixedTypeRegistration<IMenuAction>(type, TypeRepositoryActionAttributeReader.GetValue(type)));
    }

    // stupid, for test purposes. todo
    public static IKeyTypeRegistration<IMenuAction> CreateRegistrationObject<T>()
    {
        Type type = typeof(T);
        return new FixedTypeRegistration<IMenuAction>(type, TypeRepositoryActionAttributeReader.GetValue(type));
    }
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
