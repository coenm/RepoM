namespace RepoM.ActionMenu.CodeGen;

using RepoM.ActionMenu.Core.TestLib.Utils; // this is not what we want.

internal static class ThisProjectAssembly
{
    public static readonly TestAssemblyInfo Info = new(typeof(ThisProjectAssembly).Assembly);
}