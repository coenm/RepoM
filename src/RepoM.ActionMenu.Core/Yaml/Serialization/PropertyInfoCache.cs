namespace RepoM.ActionMenu.Core.Yaml.Serialization;

using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using RepoM.ActionMenu.Interface.YamlModel.Templating;

internal static class PropertyInfoCache
{
    private static readonly ConcurrentDictionary<Type, PropertyInfo[]> _cache = new();

    public static PropertyInfo[] GetPropertyInfos(Type type)
    {
        return _cache.GetOrAdd(type, static t => t
               .GetProperties(true)
               .Where(propertyInfo =>
                   propertyInfo is { CanWrite: true, CanRead: true, }
                   &&
                   typeof(EvaluateObjectBase).GetTypeInfo().IsAssignableFrom(propertyInfo.PropertyType.GetTypeInfo()))
                .ToArray());
    }
}
