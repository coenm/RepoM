# AzureDevOps

The AzureDevops module enables integration with one azure devops environment. The integration currently focuses on Pull Requests.

To use this module, make sure it is enabled in RepoM by opening the menu and navigate to 'Plugins'. After enabling or disabling a plugin, you should restart RepoM.

## Configuration

This plugin has specific configuration stored in the following directory `%APPDATA%/RepoM/Module/`. This configuration file should be edit manually. The safest way to do this is, is when RepoM is not running.

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

### Properties

- `PersonalAccessToken`: Personal access token (PAT) to access Azure Devops. The PAT should be granted access to read and write pull requests.
To create a PAT, goto `https://dev.azure.com/[my-organisation]/_usersSettings/tokens`.
- `BaseUrl`: The base url of azure devops for your organisation (i.e. `https://dev.azure.com/[my-organisation]/`).

This module contains the following methods, variables and/or constants:

- [`azure-devops-create-pr@1`](#azure-devops-create-pr1)

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
- `include-work-items`: Boolean specifying if work items should be included in the PR. RepoM will try to resolve the work items by looping through the commit messages. ([Predicate](repository_action_types.md#predicate))
- `open-in-browser`: Boolean specifying if the Pull request should be opened in the browser after creation. ([Predicate](repository_action_types.md#predicate))
- `context`: The context in which the action is available. ([Context](repository_action_types.md#context))
- `active`: Whether the menu item is enabled. ([Predicate](repository_action_types.md#predicate))
- `auto-complete`: Auto complete options. 
  - `merge-strategy`: The merge strategy. Possible values are `no-fast-forward`, `squash`, `rebase`, and `rebase-merge`.
  - `delete-source-branch`: Boolean specifying if the source branch should be deleted after completion. ([Predicate](repository_action_types.md#predicate))
  - `transition-work-items`: Boolean specifying if related work items should be transitioned to the next state. ([Predicate](repository_action_types.md#predicate))

### Example

snippet: azure-devops-create-pr@1-scenario01

snippet: azure-devops-create-pr@1-scenario02

snippet: azure-devops-create-pr@1-scenario03

