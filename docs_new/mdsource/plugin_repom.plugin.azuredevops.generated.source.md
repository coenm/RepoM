# AzureDevOps

The AzureDevops module enables integration with one azure devops environment. The integration currently focuses on Pull Requests.

To use this module, make sure it is enabled in RepoM by opening the menu and navigate to 'Plugins'. When enabling or disabling a plugin, you should restart RepoM.

- ProjectName: RepoM.Plugin.AzureDevOps
- PluginName: AzureDevOps
- PluginDescription: Integration with Azure Devops providing fetching and creating pull requests.
- PluginMarkdownDescription: The AzureDevops module enables integration with one azure devops environment. The integration currently focuses on Pull Requests.

This module contains the following methods, variables and/or constants:

## azure-devops-create-pr@1

Action menu item to create a pull request in Azure Devops.

Properties:

- `name`: Menu item title. ([Text](repository_action_types.md#text))
- `project-id`: The azure devops project id. ([Text](repository_action_types.md#text))
- `pr-title`: Pull Request title. When not provided, the title will be defined based on the branch name.
Title will be the last part of the branchname split on `/`, so `feature/123-testBranch` will result in title `123-testBranch` ([Text](repository_action_types.md#text))
- `to-branch`: Name of the branch the pull request should be merged into. For instance `develop`, or `main`. ([Text](repository_action_types.md#text))
- `reviewer-ids`: List of reviewer ids. The id should be a valid Azure DevOps user id (ie. GUID). (List<Text>)
- `draft-pr`: Boolean specifying if th PR should be marked as draft. ([Predicate](repository_action_types.md#predicate))
- `include-work-items`: Boolean specifying if workitems should be included in the PR. RepoM will try to resolve the workitems by looping through the commit messages. ([Predicate](repository_action_types.md#predicate))
- `open-in-browser`: Boolean specifying if the Pull request should be opened in the browser after creation. ([Predicate](repository_action_types.md#predicate))
- `context`: The context in which the action is available. ([Context](repository_action_types.md#context))
- `active`: Whether the menu item is enabled. ([Predicate](repository_action_types.md#predicate))
- `auto-complete`: Auto complete options. Please take a look at the same for more information (AutoCompleteOptionsV1, optional)

### Example

snippet: azure-devops-create-pr@1-scenario01

snippet: azure-devops-create-pr@1-scenario02

snippet: azure-devops-create-pr@1-scenario03

