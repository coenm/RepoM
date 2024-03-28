namespace RepoM.ActionMenu.CodeGen.Misc;

using System.Linq;
using Microsoft.CodeAnalysis;

public static class SymbolExtensions
{
    public static AttributeData? FindAttribute<T>(this ISymbol symbol)
    {
        return symbol.GetAttributes().FirstOrDefault(x => x.AttributeClass!.Name == typeof(T).Name);
    }
}