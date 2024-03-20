namespace RepoM.ActionMenu.CodeGenDummyLibrary.ActionMenu.Context;

using JetBrains.Annotations;
using RepoM.ActionMenu.Interface.ActionMenuFactory;
using RepoM.ActionMenu.Interface.Attributes;
using RepoM.ActionMenu.Interface.Scriban;

/// <summary>
/// Provides statistical information accessible through `statistics`.
/// </summary>
[UsedImplicitly]
[ActionMenuContext("statistics")]
internal class UsageVariables : TemplateContextRegistrationBase
{
    /// <summary>
    /// Gets the number of actions performed on the current repository.
    /// </summary>
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
        return context.Repository.Name.Length;
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
        return 523;
    }
}