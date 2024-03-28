namespace RepoM.ActionMenu.Core.Yaml.Serialization;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

internal static class YamlDotNetExtensions
{
    public static IEnumerable<PropertyInfo> GetProperties(this Type type, bool includeNonPublic)
    {
        BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Public;

        if (includeNonPublic)
        {
#pragma warning disable S3011
            bindingFlags |= BindingFlags.NonPublic;
#pragma warning restore S3011
        }

        return type.IsInterface
            ? new[] { type, }.Concat(type.GetInterfaces()).SelectMany(i => i.GetProperties(bindingFlags))
            : type.GetProperties(bindingFlags);
    }
}