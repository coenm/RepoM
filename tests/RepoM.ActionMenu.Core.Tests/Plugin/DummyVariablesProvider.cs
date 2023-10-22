namespace RepoM.ActionMenu.Core.Tests.Plugin;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using RepoM.ActionMenu.Interface.Scriban;

internal partial class DummyVariablesProvider : TemplateContextRegistrationBase
{
    private readonly IDummyService _dummyService;

    public DummyVariablesProvider(IDummyService dummyService)
    {
        _dummyService = dummyService ?? throw new ArgumentNullException(nameof(dummyService));
    }

    public IEnumerable GetConfigurations()
    {
        return X();
    }

    public IEnumerable Configurations
    {
        get
        {
            return X();
        }
    }

    private IEnumerable X()
    {
        List<DummyConfig> xx = new();

        var xxx = _dummyService.GetValues();

        foreach (DummyConfig x in _dummyService.GetValues())
        {
            xx.Add(x);
        }

        return xx;
    }
}