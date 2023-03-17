# AzureDevOps

*todo*

## azure-devops-get-prs@1 <!-- include: _plugins.azuredevops.action. path: /docs/mdsource/_plugins.azuredevops.action.include.md -->

This action results in zero or more items in the contextmenu. For each open pullrequest for the given repository, it will show an action to go to the specific PullRequest in your favorite webbrowser.
The AzureDevOps plugin is required.

Custom properties:

- ShowWhenEmpty: Show a menu item when no pull request found (optional, boolean/string, evaluated, default true)
- RepositoryId: The DevOps git repository id (optional, string, evaluated, default empty)
- ProjectId: The DevOps Project id (required, string, evaluated)

Example:

<!-- snippet: RepositoryActionsAzureDevopsGetPrs01 -->
<a id='snippet-repositoryactionsazuredevopsgetprs01'></a>
```yaml
repository-actions:
  actions:
  - type: azure-devops-get-prs@1
    active: true
    variables: []
    show-when-empty: true
    repository-id: ''
    project-id: ''

  - type: azure-devops-get-prs@1
    repository-id: ''
```
<sup><a href='/tests/RepoM.Plugin.AzureDevOps.Tests/DocumentationFiles/AzureDevopsGetPrs.testfile.yaml#L3-L17' title='Snippet source file'>snippet source</a> | <a href='#snippet-repositoryactionsazuredevopsgetprs01' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->
<!-- endInclude -->
