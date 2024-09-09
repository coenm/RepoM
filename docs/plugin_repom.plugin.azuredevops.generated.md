# AzureDevOps

The AzureDevops module enables integration with one azure devops environment. The integration currently focuses on Pull Requests.

To use this module, make sure it is enabled in RepoM by opening the menu and navigate to 'Plugins'. After enabling or disabling a plugin, you should restart RepoM.

- ProjectName: RepoM.Plugin.AzureDevOps
- PluginName: AzureDevOps
- PluginDescription: Integration with Azure Devops providing fetching and creating pull requests.
- PluginMarkdownDescription: The AzureDevops module enables integration with one azure devops environment. The integration currently focuses on Pull Requests.

## Configuration

The configuration. TODO

This module contains the following methods, variables and/or constants:

## azure-devops-create-pr@1

Action menu item to create a pull request in Azure Devops.

Properties:

- `name`: Menu item title. ([Text](repository_action_types.md#text))
- `project-id`: The azure devops project id. ([Text](repository_action_types.md#text))
- `pr-title`: Pull Request title. When not provided, the title will be defined based on the branch name.
Title will be the last part of the branchname split on `/`, so `feature/123-testBranch` will result in title `123-testBranch` ([Text](repository_action_types.md#text))
- `to-branch`: Name of the branch the pull request should be merged into. For instance `develop`, or `main`. ([Text](repository_action_types.md#text))
- `reviewer-ids`: List of reviewer ids. The id should be a valid Azure DevOps user id (i.e. GUID). (List<Text>)
- `draft-pr`: Boolean specifying if th PR should be marked as draft. ([Predicate](repository_action_types.md#predicate))
- `include-work-items`: Boolean specifying if workitems should be included in the PR. RepoM will try to resolve the workitems by looping through the commit messages. ([Predicate](repository_action_types.md#predicate))
- `open-in-browser`: Boolean specifying if the Pull request should be opened in the browser after creation. ([Predicate](repository_action_types.md#predicate))
- `context`: The context in which the action is available. ([Context](repository_action_types.md#context))
- `active`: Whether the menu item is enabled. ([Predicate](repository_action_types.md#predicate))
- `auto-complete`: Auto complete options. Please take a look at the same for more information (AutoCompleteOptionsV1, optional)

### Example

<!-- snippet: azure-devops-create-pr@1-scenario01 -->
<a id='snippet-azure-devops-create-pr@1-scenario01'></a>
```yaml
- type: azure-devops-create-pr@1
  project-id: "{{ project_id }}"
  name: Create feature to develop ({{ repository.branch | string.replace "feature/" "" | string.strip | string.truncate 20 ".." }})
  pr-title: 'Release {{ now }}'
  to-branch: develop
  reviewer-ids:
  - "{{ devops_guid_reviewer_1 }}"
  - "33333333-F973-4BE7-B39A-A9F85B18C75E"
  draft-pr: false
  include-work-items: true
  open-in-browser: true
  auto-complete:
    merge-strategy: Squash
    delete-source-branch: true
    transition-work-items: true
  active: 'repository.branch | string.starts_with "feature/"'
```
<sup><a href='/tests/RepoM.Plugin.AzureDevOps.Tests/ActionMenu/IntegrationTests/AzureDevopsCreatePrV1Tests.CreatePullRequestScenario01.testfile.yaml#L10-L29' title='Snippet source file'>snippet source</a> | <a href='#snippet-azure-devops-create-pr@1-scenario01' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

<!-- snippet: azure-devops-create-pr@1-scenario02 -->
<a id='snippet-azure-devops-create-pr@1-scenario02'></a>
```yaml
- type: azure-devops-create-pr@1
  project-id: "{{ project_id }}"
  name: Complete feature
  pr-title: 'Feature {{ repository.branch | string.replace "feature/" "" }}'
  to-branch: develop
  reviewer-ids:
  - "{{ devops_guid_reviewer_1 }}"
  draft-pr: repository.banch == "develop"
  active: true
```
<sup><a href='/tests/RepoM.Plugin.AzureDevOps.Tests/ActionMenu/IntegrationTests/AzureDevopsCreatePrV1Tests.CreatePullRequestScenario01.testfile.yaml#L31-L43' title='Snippet source file'>snippet source</a> | <a href='#snippet-azure-devops-create-pr@1-scenario02' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

<!-- snippet: azure-devops-create-pr@1-scenario03 -->
<a id='snippet-azure-devops-create-pr@1-scenario03'></a>
```yaml
- type: azure-devops-create-pr@1
  project-id: "{{ project_id }}"
  to-branch: develop
```
<sup><a href='/tests/RepoM.Plugin.AzureDevOps.Tests/ActionMenu/IntegrationTests/AzureDevopsCreatePrV1Tests.CreatePullRequestScenario01.testfile.yaml#L45-L51' title='Snippet source file'>snippet source</a> | <a href='#snippet-azure-devops-create-pr@1-scenario03' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

