namespace RepoM.Plugin.AzureDevOps.ActionMenu.Model.ActionMenus.CreatePullRequest;

using System.ComponentModel.DataAnnotations;
using JetBrains.Annotations;
using RepoM.ActionMenu.Interface.YamlModel.Templating;
using RepoM.Api.IO.ModuleBasedRepositoryActionProvider.Data;

/// <summary>
/// Auto complete options.
/// </summary>
[UsedImplicitly]
internal class AutoCompleteOptionsV1
{
    /// <summary>
    /// The merge strategy. Possible values are `NoFastForward`, `Squash`, `Rebase`, and `RebaseMerge`.
    /// </summary>
    [Required]
    [PropertyType(typeof(MergeStrategyV1))]
    [PropertyDefaultTypedValue<MergeStrategyV1>(MergeStrategyV1.NoFastForward)]
    public MergeStrategyV1 MergeStrategy { get; set; } = MergeStrategyV1.NoFastForward; // todo enum?

    /// <summary>
    /// Boolean specifying if the source branche should be deleted afer completion.
    /// </summary>
    [Required]
    [Predicate(true)]
    public Predicate DeleteSourceBranch { get; set; } = true;

    /// <summary>
    /// Boolean specifying if related workitems should be transitioned to the next state.
    /// </summary>
    [Required]
    [Predicate(true)]
    public Predicate TransitionWorkItems { get; set; } = true;
}