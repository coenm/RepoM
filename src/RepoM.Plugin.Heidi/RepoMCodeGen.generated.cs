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

namespace RepoM.Plugin.Heidi.ActionMenu.Context
{
    using RepoM.ActionMenu.Interface.Scriban;

    partial class HeidiDbVariables
    {
        public override void RegisterFunctions(RepoM.ActionMenu.Interface.Scriban.IContextRegistration contextRegistration)
        {
            IContextRegistration heidiDbRegistration = contextRegistration.CreateOrGetSubRegistration("heidi"); // todo do not remove this.
            heidiDbRegistration.RegisterFunction("databases", (Func<RepoM.ActionMenu.Interface.ActionMenuFactory.IActionMenuGenerationContext, System.Collections.IEnumerable>)GetDatabases);
        }
    }
}
