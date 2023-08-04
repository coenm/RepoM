# AzureDevOps

The AzureDevops module enables integration with one azure devops environment. The integration currently focusses on Pull Requests.

To use this module, make sure it is enabled in RepoM by opening the menu and navigate to 'Plugins'. When enabling or disabling a plugin, you should restart RepoM. <!-- singleLineInclude: _plugin_enable. path: /docs/mdsource/_plugin_enable.include.md -->

## Configuration <!-- include: DocsModuleSettingsTests.DocsModuleSettings_AzureDevOpsPackage#desc.verified.md -->

This plugin has specific configuration stored in a separate configuration file stored in `%APPDATA%/RepoM/Module/` directory. This configuration file should be edit manually. The safest way to do this is, is when RepoM is not running.

The following default configuration is used:

```json
{
  "Version": 1,
  "Settings": {
    "PersonalAccessToken": null,
    "BaseUrl": null
  }
}
```

Properties:

- `PersonalAccessToken`: Personal access token (PAT) to access Azure Devops. The PAT should be granted access to `todo` rights.
To create a PAT, goto `https://dev.azure.com/[my-organisation]/_usersSettings/tokens`.
- `BaseUrl`: The base url of azure devops for your organisation (ie. `https://dev.azure.com/[my-organisation]/`). <!-- endInclude -->

## azure-devops-create-prs@1 <!-- include: _plugins.azuredevops.action. path: /docs/mdsource/_plugins.azuredevops.action.include.md -->

Action menu item to create a pull request in Azure Devops. <!-- include: DocsRepositoryActionsTests.DocsRepositoryActionsSettings_RepositoryActionAzureDevOpsCreatePullRequestsV1.verified.md -->

Action specific properties:

- `title`: TODO (optional)
- `pr-title`: TODO (optional)
- `to-branch`: TODO
- `reviewer-ids`: TODO
- `draft-pr`: TODO
- `include-work-items`: TODO
- `open-in-browser`: TODO
- `auto-complete`: TODO <!-- endInclude -->

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

This action results in zero or more items in the contextmenu. For each open pullrequest for the given repository, it will show an action to go to the specific PullRequest in your favorite webbrowser. <!-- include: DocsRepositoryActionsTests.DocsRepositoryActionsSettings_RepositoryActionAzureDevOpsGetPullRequestsV1.verified.md -->

Action specific properties:

- `repo-id`: The repository Id. (optional, evaluated, string)
- `show-when-empty`: When no pull requests are available, this property is used to determine if no or a message item is showed. (optional, evaluated, string) <!-- endInclude -->

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
