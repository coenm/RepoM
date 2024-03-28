namespace RepoM.ActionMenu.CodeGen.Misc;

using System.Collections.Generic;
using Microsoft.CodeAnalysis;

public static class CompilationExtensions
{
    public static IEnumerable<ITypeSymbol> GetTypes(this Compilation compilation)
    {
        foreach (ISymbol type in compilation.GetSymbolsWithName(_ => true, SymbolFilter.Type))
        {
            if (type is ITypeSymbol typeSymbol)
            {
                yield return typeSymbol;
            }
        }
    }
}