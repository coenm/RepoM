## azure-devops-create-prs@1

include: DocsRepositoryActionsTests.DocsRepositoryActionsSettings_RepositoryActionAzureDevOpsCreatePullRequestsV1.verified.md

Example:

snippet: RepositoryActionsAzureDevopsCreatePrs01

## azure-devops-get-prs@1

This action results in zero or more items in the contextmenu. For each open pullrequest for the given repository, it will show an action to go to the specific PullRequest in your favorite webbrowser.
The AzureDevOps plugin is required.

include: DocsRepositoryActionsTests.DocsRepositoryActionsSettings_RepositoryActionAzureDevOpsGetPullRequestsV1.verified.md

Custom properties:

- ShowWhenEmpty: Show a menu item when no pull request found (optional, boolean/string, evaluated, default true)
- RepositoryId: The DevOps git repository id (optional, string, evaluated, default empty)
- ProjectId: The DevOps Project id (required, string, evaluated)

Example:

snippet: RepositoryActionsAzureDevopsGetPrs01
