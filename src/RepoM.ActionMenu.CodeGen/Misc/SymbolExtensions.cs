namespace RepoM.ActionMenu.CodeGen.Misc;

using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

public static class SymbolExtensions
{
    public static AttributeData? FindAttribute<T>(this ISymbol symbol)
    {
        return symbol.GetAttributes().FirstOrDefault(x => x.AttributeClass!.Name == typeof(T).Name);
    }

    public static List<AttributeData> FindAttributes<T>(this ISymbol symbol)
    {
        return symbol.GetAttributes()
                     .Where(x => x.AttributeClass!.Name == typeof(T).Name)
                     .ToList();
    }
}