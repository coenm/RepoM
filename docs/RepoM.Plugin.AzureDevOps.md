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

- `project-id`: The azure devops project id. (required, evaluated, string)
- `title`: Menu item title. When not provided, a title will be generated.
This property will be used instead of the Name property. (optional, string)
- `pr-title`: Pull Request title. When not provided, the title will be defined based on the branch name.
Title will be the last part of the branchname split on `/`, so `feature/123-testBranch` will result in title `123-testBranch` (optional, string)
- `to-branch`: Name of the branch the pull request should be merged into. For instance `develop`, or `main`. (required, string)
- `reviewer-ids`: List of reviewer ids. The id should be a valid Azure DevOps user id (ie. GUID). (optional, list`1)
- `draft-pr`: Boolean specifying if th PR should be marked as draft. (required, boolean, default: `false`)
- `include-work-items`: Boolean specifying if workitems should be included in the PR. The workitems will be found by using the commit messages. (required, boolean, default: `true`)
- `open-in-browser`: Boolean specifying if the Pull request should be opened in the browser after creation. (required, boolean, default: `false`)
- `auto-complete`: Auto complete options. Please take a look at the same for more information (required, repositoryactionazuredevopscreatepullrequestsautocompleteoptionsv1) <!-- endInclude -->

Example:

<!-- snippet: RepositoryActionsAzureDevopsCreatePrs01 -->
<a id='snippet-repositoryactionsazuredevopscreateprs01'></a>
```yaml
repository-actions:
  actions:
  # Create PR
  - type: azure-devops-create-prs@1
    project-id: ''
    to-branch: develop
    reviewer-ids: 
    - "GUID"

  # Create PR with auto-complete enabled
  - type: azure-devops-create-prs@1
    project-id: ''
    to-branch: develop
    reviewer-ids:
    - "GUID"
    auto-complete:
      enabled: true
      merge-strategy: "Squash"

  # Create PR with all settings
  - type: azure-devops-create-prs@1
    project-id: ''
    title: 'Create PR'
    # When no pr-title provided it will be generated based on convention.
    # Title will be the last part of the branchname split on '/'. 
    # For example: feature/testBranch will result in a PR title of 'testBranch'.
    pr-title: 'PR title' 
    to-branch: develop
    reviewer-ids:
    - "GUID"
    draft-pr: true
    include-work-items: true
    open-in-browser: false
    auto-complete:
      enabled: true
      merge-strategy: "NoFastForward" # You can choose from: "NoFastForward", "Squash", "Rebase" and "RebaseMerge"
      deleteSource-branch: true
      transition-work-items: true
```
<sup><a href='/tests/RepoM.Plugin.AzureDevOps.Tests/DocumentationFiles/AzureDevopsCreatePrs.testfile.yaml#L3-L44' title='Snippet source file'>snippet source</a> | <a href='#snippet-repositoryactionsazuredevopscreateprs01' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

## azure-devops-get-prs@1

This action results in zero or more items in the contextmenu. For each open pullrequest for the given repository, it will show an action to go to the specific PullRequest in your favorite webbrowser. <!-- include: DocsRepositoryActionsTests.DocsRepositoryActionsSettings_RepositoryActionAzureDevOpsGetPullRequestsV1.verified.md -->

Action specific properties:

- `project-id`: The azure devops project id. (required, evaluated, string)
- `repository-id`: The repository Id. If not provided, the repository id is located using the remote url. (optional, evaluated, string)
- `show-when-empty`: When no pull requests are available, this property is used to determine if no or a message item is showed. (optional, evaluated, string, default: `true`) <!-- endInclude -->

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
