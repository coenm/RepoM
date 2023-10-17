//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
#nullable enable

using System;

namespace RepoM.ActionMenu.Core.Model.Functions
{
    partial class FileFunctions
    {
        protected sealed override void RegisterFunctions()
        {
            RegisterFunction("find_files", (Func<RepoM.ActionMenu.Core.Model.ActionMenuGenerationContext, Scriban.Parsing.SourceSpan, string, string, string[]>)FindFiles);
            RegisterFunction("file_exists", (Func<RepoM.ActionMenu.Core.Model.ActionMenuGenerationContext, string, bool>)FileExists);
            RegisterFunction("dir_exists", (Func<RepoM.ActionMenu.Core.Model.ActionMenuGenerationContext, string, bool>)DirectoryExists);
        }
    }
}

namespace RepoM.ActionMenu.Core.Model.Functions
{
    partial class RepositoryFunctions
    {
        protected sealed override void RegisterFunctions()
        {
            RegisterConstant("name", Name);
            RegisterConstant("path", Path);
            RegisterConstant("safe_path", SafePath);
            RegisterConstant("branch", CurrentBranch);
            RegisterConstant("branches", Branches);
            RegisterConstant("local_branches", LocalBranches);
        }
    }
}
