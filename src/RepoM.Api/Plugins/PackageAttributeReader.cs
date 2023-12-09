namespace RepoM.Api.Plugins;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Mono.Cecil;
using RepoM.Core.Plugin.AssemblyInformation;

public static class PackageAttributeReader
{
    public static PackageAttribute? ReadPackageDataWithoutLoadingAssembly(Stream stream)
    {
        try
        {
            using var assembly = AssemblyDefinition.ReadAssembly(stream);
            CustomAttribute? attribute = assembly.CustomAttributes.SingleOrDefault(a => a.AttributeType.FullName == typeof(PackageAttribute).FullName);

            if (attribute == null)
            {
                return null;
            }

            IList<CustomAttributeArgument> constructorArguments = attribute.ConstructorArguments;

            if (constructorArguments.Count != 2)
            {
                return null;
            }

            var arg0 = constructorArguments[0].Value.ToString();
            var arg1 = constructorArguments[1].Value.ToString();
            var arg2 = constructorArguments[2].Value.ToString();
            return new PackageAttribute(arg0!, arg1!, arg2!);
        }
        catch (Exception)
        {
            return null!;
        }
    }
}