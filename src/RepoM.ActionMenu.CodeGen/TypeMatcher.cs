namespace RepoM.ActionMenu.CodeGen;

using System;
using System.Linq;
using Microsoft.CodeAnalysis;

public static class TypeMatcher
{
    public static bool TypeSymbolMatchesType(ITypeSymbol typeSymbol, Type type, Compilation compilation)
    {
        return GetTypeSymbolForType(type, compilation).Equals(typeSymbol);
    }

    public static INamedTypeSymbol GetTypeSymbolForType(Type type, Compilation compilation)
    {
        if (!type.IsConstructedGenericType)
        {
            return compilation.GetTypeByMetadataName(type.FullName!);
        }

        // get all typeInfo's for the Type arguments 
        var typeArgumentsTypeInfos = type.GenericTypeArguments.Select(a => GetTypeSymbolForType(a, compilation));

        var openType = type.GetGenericTypeDefinition();
        var typeSymbol = compilation.GetTypeByMetadataName(openType.FullName!);
        return typeSymbol.Construct(typeArgumentsTypeInfos.ToArray<ITypeSymbol>());
    }
}