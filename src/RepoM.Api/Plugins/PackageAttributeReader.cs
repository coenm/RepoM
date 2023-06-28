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
                // Invalid number of constructor arguments;
                return null;
            }

            var key = constructorArguments[0].Value.ToString();
            var value = constructorArguments[1].Value.ToString();
            return new PackageAttribute(key!, value!);
        }
        catch (Exception)
        {
            return null!;
        }
    }
}