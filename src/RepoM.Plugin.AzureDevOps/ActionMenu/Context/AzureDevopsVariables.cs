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

[UsedImplicitly]
[ActionMenuModule("azure_devops")]
internal partial class AzureDevopsVariables : TemplateContextRegistrationBase
{
    private readonly IAzureDevOpsPullRequestService _service;
    private readonly ILogger _logger;

    public AzureDevopsVariables(IAzureDevOpsPullRequestService service, ILogger logger)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [ActionMenuMember("prs")]
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