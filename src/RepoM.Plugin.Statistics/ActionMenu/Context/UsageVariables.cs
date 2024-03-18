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
[ActionMenuContext("statistics")]
internal partial class UsageVariables : TemplateContextRegistrationBase
{
    private readonly IStatisticsService _service;

    public UsageVariables(IStatisticsService service)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
    }

    /// <summary>
    /// Gets the number of actions performed on the current repository.
    /// </summary>
    /// <param name="context">The scriban context.</param>
    /// <returns>Number of actions performed on the current repository.</returns>
    /// <example>
    /// <usage/>
    /// <code>
    /// repo_call_count = statistics.count;
    /// </code>
    /// </example>
    [ActionMenuContextMember("count")]
    public int GetCount(IActionMenuGenerationContext context)
    {
        return _service.GetRecordings(context.Repository).Count;
    }

    /// <summary>
    /// Gets the number of actions performed on all repositories known in RepoM.
    /// </summary>
    /// <returns>Number of actions performed on any known repository.</returns>
    /// <example>
    /// <usage/>
    /// <code>
    /// repo_call_count = statistics.overall_count;
    /// </code>
    /// </example>
    [ActionMenuContextMember("overall_count")]
    public int GetOverallCount()
    {
        return _service.GetTotalRecordingCount();
    }
}