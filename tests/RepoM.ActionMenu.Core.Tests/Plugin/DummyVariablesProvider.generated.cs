namespace RepoM.ActionMenu.Core.Tests.Plugin;

using System;
using RepoM.ActionMenu.Interface.Scriban;

partial class DummyVariablesProvider
{
    public override void RegisterFunctions(IContextRegistration contextRegistration)
    {
        IContextRegistration dummyConfig = contextRegistration.CreateOrGetSubRegistration("dummy");
        dummyConfig.RegisterFunction("configurations_func", GetConfigurationsFunc);
        dummyConfig.RegisterVariable("configurations_prop", ConfigurationsProp);
        dummyConfig.RegisterFunction("configuration_interface_method", (Func<RepoM.ActionMenu.Interface.ActionMenuFactory.IActionMenuGenerationContext, int, string, bool, System.Collections.IEnumerable>)GetConfigInterfaceArg);
    }
}