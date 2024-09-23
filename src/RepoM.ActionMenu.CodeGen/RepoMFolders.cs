namespace RepoM.ActionMenu.CodeGen;

using System.IO;
using RepoM.ActionMenu.Core.TestLib.Utils;

internal static class RepoMFolders
{
    /// <summary>
    /// Solution directory (root of the git repository)
    /// </summary>
    public static readonly string Root = ThisProjectAssembly.Info.GetSolutionDirectory();

    /// <summary>
    /// Git folder (.git)
    /// </summary>
    public static readonly string Git = Path.Combine(Root, ".git");

    /// <summary>
    /// Source folder (src)
    /// </summary>
    public static readonly string Source = Path.Combine(Root, "src");

    /// <summary>
    /// Documentation folder (docs)
    /// </summary>
    public static readonly string Documentation = Path.Combine(Root, "docs");

    /// <summary>
    /// Old Documentation folder (docs_old)
    /// </summary>
    public static readonly string DocumentationOld = Path.Combine(Documentation, "_old");

    /// <summary>
    /// Markdown source folder for documentation
    /// </summary>
    public static readonly string DocumentationMarkDownSource = Path.Combine(Documentation, "mdsource");
}