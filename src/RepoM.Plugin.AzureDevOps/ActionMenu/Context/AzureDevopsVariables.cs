namespace RepoM.Plugin.AzureDevOps.ActionMenu.Context;

using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using RepoM.ActionMenu.Interface.ActionMenuFactory;
using RepoM.ActionMenu.Interface.Attributes;
using RepoM.ActionMenu.Interface.Scriban;
using RepoM.Plugin.AzureDevOps.Internal;

/// <summary>
/// Provides Azure Devops functions through `azure_devops`.
/// </summary>
[UsedImplicitly]
[ActionMenuContext("azure_devops")]
internal partial class AzureDevopsVariables : TemplateContextRegistrationBase
{
    private readonly IAzureDevOpsPullRequestService _service;
    private readonly ILogger _logger;

    public AzureDevopsVariables(IAzureDevOpsPullRequestService service, ILogger logger)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Get pull requests for the given project. The result is an enumeration of <see cref="PullRequest">PullRequest</see>.
    /// </summary>
    /// <param name="projectId">The azure devops project id. Cannot be null or empty.</param>
    /// <returns>Returns an enumeration of pull requests for the selected repository (or an empty enumeration when no pull requests are found).</returns>
    /// <example>
    /// <usage/>
    /// Get all pull requests for the selected repository in a given devops project:
    /// <code>
    /// devops_project_id = "805ACF64-0F06-47EC-96BF-E830895E2740";
    /// prs = azure_devops.get_pull_requests(devops_project_id);
    /// </code>
    /// <result/>
    /// As a result, the variable `prs` could contain two pull requests with the following dummy data:
    /// <code-file language='yaml' filename='azure_devops.get_pull_requests.verified.yaml' />
    /// <repository-action-sample/>
    /// <code-file language='yaml' filename='azure_devops.get_pull_requests.actionmenu.yaml' />
    /// </example>
    [ActionMenuContextMember("get_pull_requests")]
    public IEnumerable GetPullRequests(IActionMenuGenerationContext context, string projectId)
    {
        if (string.IsNullOrWhiteSpace(projectId))
        {
            yield break;
        }
        
        List<PullRequest>? pullRequests;

        try
        {
            pullRequests = _service.GetPullRequests(context.Repository, projectId, null!);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Could not grab pull requests.");
            yield break;
        }

        if (pullRequests == null)
        {
            yield break;
        }

        foreach (PullRequest pr in pullRequests)
        {
            yield return pr;
        }
    }
}