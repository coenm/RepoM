namespace RepoM.Plugin.Statistics.ActionMenu.Context;

using System;
using JetBrains.Annotations;
using RepoM.ActionMenu.Interface.ActionMenuFactory;
using RepoM.ActionMenu.Interface.Attributes;
using RepoM.ActionMenu.Interface.Scriban;
using RepoM.Plugin.Statistics;

/// <summary>
/// Provides statistical information accessible through `statistics`.
/// </summary>
[UsedImplicitly]
[ActionMenuModule("statistics")]
internal partial class UsageVariables : TemplateContextRegistrationBase
{
    private readonly IStatisticsService _service;

    public UsageVariables(IStatisticsService service)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
    }

    /// <summary>
    /// Gets the number of actions performed on the current repository.
    /// <para>Module settings might affect the result.</para>
    /// </summary>
    /// <returns>Number of actions performed on the current repository.</returns>
    [ActionMenuMember("count")]
    public int GetCount(IActionMenuGenerationContext context)
    {
        return _service.GetRecordings(context.Repository).Count;
    }

    /// <summary>
    /// Gets the number of actions performed on all repositories known in RepoM.
    /// <para>Module settings might affect the result.</para>
    /// </summary>
    /// <returns>Number of actions performed on any known repository.</returns>
    [ActionMenuMember("overall_count")]
    public int GetOverallCount => _service.GetTotalRecordingCount();
}