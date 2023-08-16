namespace RepoM.Core.Plugin;

using System;
using System.Reflection;
using RepoM.Core.Plugin.RepositoryOrdering.Configuration;
using SimpleInjector;

public static class ContainerExtensions
{
    public static void RegisterComparerConfigurationForType<T>(this Container container) where T : IRepositoriesComparerConfiguration
    {
        RegisterComparerConfigurationForType(container, typeof(T));
    }

    public static void RegisterComparerConfigurationForType(this Container container, Type type)
    {
        container.Collection.AppendInstance<IKeyTypeRegistration<IRepositoriesComparerConfiguration>>(new FixedTypeRegistration<IRepositoriesComparerConfiguration>(type, TypeValueFieldReader.GetTypeValue(type)));
    }

    public static void RegisterScorerConfigurationForType<T>(this Container container) where T : IRepositoryScorerConfiguration
    {
        RegisterScorerConfigurationForType(container, typeof(T));
    }

    public static void RegisterScorerConfigurationForType(this Container container, Type type)
    {
        container.Collection.AppendInstance<IKeyTypeRegistration<IRepositoryScorerConfiguration>>(new FixedTypeRegistration<IRepositoryScorerConfiguration>(type, TypeValueFieldReader.GetTypeValue(type)));
    }
}

/// <summary>
/// This class 'assumes' that the type has a public field named 'TYPE_VALUE' that contains the value to be used as the type value.
/// </summary>
file static class TypeValueFieldReader
{
    public static string GetTypeValue(Type type)
    {
        FieldInfo fieldInfo = type.GetField("TYPE_VALUE") ?? throw new Exception("Could not locate field 'TYPE_VALUE'");
        return fieldInfo.GetValue(null)?.ToString() ?? throw new Exception("Could not get value of property 'TYPE_VALUE'");
    }
}