namespace RepoM.Plugin.Misc.Tests.TestFramework.AssemblyAndTypeHelpers;

using System;
using System.Reflection;

internal static class TypeExtension
{
    public static string GetTypeValue(this Type type)
    {
        FieldInfo fieldInfo = type.GetField("TYPE_VALUE") ?? throw new Exception("Could not locate field 'TYPE_VALUE'");
        return fieldInfo.GetValue(null)?.ToString() ?? throw new Exception("Could not get value of property 'TYPE_VALUE'");
    }
}