namespace RepoM.ActionMenu.CodeGen.Misc;

using System;
using System.Collections.Generic;
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
            return compilation.GetTypeByMetadataName(type.FullName!)!;
        }

        // get all typeInfo's for the Type arguments 
        IEnumerable<INamedTypeSymbol> typeArgumentsTypeInfos = type.GenericTypeArguments.Select(a => GetTypeSymbolForType(a, compilation));

        Type openType = type.GetGenericTypeDefinition();
        INamedTypeSymbol? typeSymbol = compilation.GetTypeByMetadataName(openType.FullName!);
        return typeSymbol!.Construct(typeArgumentsTypeInfos.ToArray<ITypeSymbol>());
    }
}