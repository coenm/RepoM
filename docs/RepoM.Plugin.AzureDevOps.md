# AzureDevOps

The AzureDevops module enables integration with one azure devops environment. The integration currently focusses on Pull Requests.

To use this module, make sure it is enabled in RepoM by opening the menu and navigate to 'Plugins'. When enabling or disabling a plugin, you should restart RepoM. <!-- singleLineInclude: _plugin_enable. path: /docs/mdsource/_plugin_enable.include.md -->

This plugin has specific configuration stored in a separate configuration file stored in `%APPDATA%/RepoM/Module/` directory. This configuration file should be edit manually. The safest way to do this is, is when RepoM is not running. <!-- include: DocsModuleSettingsTests.DocsModuleSettings_AzureDevOpsPackage#desc.verified.md -->

The following default configuration is used

```json
{
  "Version": 1,
  "Settings": {
    "PersonalAccessToken": null,
    "BaseUrl": null
  }
}
```
<!-- endInclude -->

Provide a `PersonalAccessToken` and a `BaseUrl`. The `BaseUrl` must end with the organisation and a slash (ie, `https://dev.azure.com/organisation/`) and the 'PAT' should be granted access to `XYZ` (TODO).

## azure-devops-create-prs@1 <!-- include: _plugins.azuredevops.action. path: /docs/mdsource/_plugins.azuredevops.action.include.md -->

This action results in zero or one item in the contextmenu. This action makes it possible to create a pullrequest for the given repository, it will show an action to go to the specific PullRequest in your favorite webbrowser.
The AzureDevOps plugin is required.

Example:

<!-- snippet: RepositoryActionsAzureDevopsCreatePrs01 -->
<a id='snippet-repositoryactionsazuredevopscreateprs01'></a>
```yaml
repository-actions:
  actions:
  # Create PR
  - type: azure-devops-create-prs@1
    projectId: ''
    toBranch: develop
    reviewerIds: 
    - "GUID"

  # Create PR with auto-complete enabled
  - type: azure-devops-create-prs@1
    projectId: ''
    toBranch: develop
    reviewerIds:
    - "GUID"
    autoComplete:
      enabled: true
      mergeStrategy: "Squash"

  # Create PR with all settings
  - type: azure-devops-create-prs@1
    projectId: ''
    title: 'Create PR'
    # When no prTitle provided it will be generated based on convention.
    # Title will be the last part of the branchname split on '/'. 
    # For example: feature/testBranch will result in a PR title of 'testBranch'.
    prTitle: 'PR title' 
    toBranch: develop
    reviewerIds:
    - "GUID"
    draftPr: true
    includeWorkItems: true
    openInBrowser: false
    autoComplete:
      enabled: true
      mergeStrategy: "NoFastForward" # You can choose from: "NoFastForward", "Squash", "Rebase" and "RebaseMerge"
      deleteSourceBranch: true
      transitionWorkItems: true
```
<sup><a href='/tests/RepoM.Plugin.AzureDevOps.Tests/DocumentationFiles/AzureDevopsCreatePrs.testfile.yaml#L3-L44' title='Snippet source file'>snippet source</a> | <a href='#snippet-repositoryactionsazuredevopscreateprs01' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

## azure-devops-get-prs@1

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
