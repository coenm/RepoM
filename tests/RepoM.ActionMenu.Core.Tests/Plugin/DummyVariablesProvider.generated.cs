namespace RepoM.ActionMenu.Core.Tests.Plugin;

using RepoM.ActionMenu.Interface.Scriban;

partial class DummyVariablesProvider
{
    public override void RegisterFunctionsAuto(IContextRegistration contextRegistration)
    {
        IContextRegistration dummyConfig = contextRegistration.CreateOrGetSubRegistration("dummy");
        dummyConfig.RegisterFunction("configurations", GetConfigurations);
        dummyConfig.RegisterVariable("configurations1", Configurations);
    }
}