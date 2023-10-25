namespace RepoM.Plugin.Statistics.ActionMenu.Context;

using System;
using JetBrains.Annotations;
using RepoM.ActionMenu.Interface.ActionMenuFactory;
using RepoM.ActionMenu.Interface.Attributes;
using RepoM.ActionMenu.Interface.Scriban;
using RepoM.Plugin.Statistics;

[UsedImplicitly]
[ActionMenuModule("statistics")]
internal partial class UsageVariables : TemplateContextRegistrationBase
{
    private readonly IStatisticsService _service;

    public UsageVariables(IStatisticsService service)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
    }

    [ActionMenuMember("count")]
    public int GetCount(IActionMenuGenerationContext context)
    {
        return _service.GetRecordings(context.Repository).Count;
    }

    [ActionMenuMember("overall_count")]
    public int GetOverallCount => _service.GetTotalRecordingCount();
}