namespace RepoM.Plugin.Heidi.ActionMenu.Context;

using System;
using System.Collections;
using JetBrains.Annotations;
using RepoM.ActionMenu.Interface.Scriban;

[UsedImplicitly]
internal partial class HeidiDbVariables : TemplateContextRegistrationBase
{
    private readonly IDatabaseConfigurationService _service;

    public HeidiDbVariables(IDatabaseConfigurationService service)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
    }

    public IEnumerable CurrentDirectoryStatic()
    {
        return _service.GetDatabases();
    }
}

partial class HeidiDbVariables
{
    public override void RegisterFunctionsAuto(IContextRegistration contextRegistration)
    {
        IContextRegistration heidiDbRegistration = contextRegistration.CreateOrGetSubRegistration("heidi-db"); // todo heidi_db?
        heidiDbRegistration.RegisterFunction("all", CurrentDirectoryStatic);
    }
}