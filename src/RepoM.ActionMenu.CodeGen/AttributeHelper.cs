namespace RepoM.ActionMenu.CodeGen;

using System.Linq;
using Microsoft.CodeAnalysis;

public static class AttributeHelper
{
    public static AttributeData? FindAttribute<T>(this ISymbol symbol)
    {
        return symbol.GetAttributes().FirstOrDefault(x => x.AttributeClass!.Name == typeof(T).Name);
    }
}