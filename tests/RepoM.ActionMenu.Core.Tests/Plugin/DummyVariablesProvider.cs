namespace RepoM.ActionMenu.Core.Tests.Plugin;

using System;
using System.Collections;
using System.Collections.Generic;
using RepoM.ActionMenu.Interface.ActionMenuFactory;
using RepoM.ActionMenu.Interface.Scriban;

internal partial class DummyVariablesProvider : TemplateContextRegistrationBase
{
    private readonly IDummyService _dummyService;

    public DummyVariablesProvider(IDummyService dummyService)
    {
        _dummyService = dummyService ?? throw new ArgumentNullException(nameof(dummyService));
    }

    public IEnumerable GetConfigurationsFunc()
    {
        return GetConfigImpl();
    }


    public IEnumerable GetConfigInterfaceArg(IActionMenuGenerationContext context, int i, string s, bool b)
    {
        return GetConfigImpl();
    }

    public IEnumerable ConfigurationsProp => GetConfigImpl();

    private IEnumerable GetConfigImpl()
    {
        List<DummyConfig> result = new();

        foreach (DummyConfig item in _dummyService.GetValues())
        {
            result.Add(item);
        }

        return result;
    }
}