# AzureDevOps

The AzureDevops module enables integration with one azure devops environment. The integration currently focuses on Pull Requests.

To use this module, make sure it is enabled in RepoM by opening the menu and navigate to 'Plugins'. When enabling or disabling a plugin, you should restart RepoM.

ProjectName: RepoM.Plugin.AzureDevOps
PluginName: AzureDevOps
PluginDescription: Integration with Azure Devops providing fetching and creating pull requests.
PluginMarkdownDescription: The AzureDevops module enables integration with one azure devops environment. The integration currently focuses on Pull Requests.

This module contains the following methods, variables and/or constants:

[## `azure-devops-create-pr@1`](#azure-devops-create-pr@1)

Action menu item to create a pull request in Azure Devops.

Action specific properties:
- `name` (RepoM.ActionMenu.Interface.YamlModel.Templating.Text): Menu item title. When not provided, a title will be generated.
This property will be used instead of the Name property.
- `project-id` (RepoM.ActionMenu.Interface.YamlModel.Templating.Text): The azure devops project id.
- `pr-title` (RepoM.ActionMenu.Interface.YamlModel.Templating.Text): Pull Request title. When not provided, the title will be defined based on the branch name.
Title will be the last part of the branchname split on `/`, so `feature/123-testBranch` will result in title `123-testBranch`
- `to-branch` (RepoM.ActionMenu.Interface.YamlModel.Templating.Text): Name of the branch the pull request should be merged into. For instance `develop`, or `main`.
- `reviewer-ids` (System.Collections.Generic.List<RepoM.ActionMenu.Interface.YamlModel.Templating.Text>): List of reviewer ids. The id should be a valid Azure DevOps user id (ie. GUID).
- `draft-pr` (RepoM.ActionMenu.Interface.YamlModel.Templating.Predicate): Boolean specifying if th PR should be marked as draft.
- `include-work-items` (RepoM.ActionMenu.Interface.YamlModel.Templating.Predicate): Boolean specifying if workitems should be included in the PR. RepoM will try to resolve the workitems by looping through the commit messages.
- `open-in-browser` (RepoM.ActionMenu.Interface.YamlModel.Templating.Predicate): Boolean specifying if the Pull request should be opened in the browser after creation.
- `context` (RepoM.ActionMenu.Interface.YamlModel.ActionMenus.Context?): 
- `active` (RepoM.ActionMenu.Interface.YamlModel.Templating.Predicate): 
- `auto-complete` (RepoM.Plugin.AzureDevOps.ActionMenu.Model.ActionMenus.CreatePullRequest.AutoCompleteOptionsV1?): Auto complete options. Please take a look at the same for more information
[## `azure-devops-create-prs@1`](#azure-devops-create-prs@1)

Action menu item to create a pull request in Azure Devops.

Action specific properties:
- `project-id` (string?): The azure devops project id.
- `title` (string?): Menu item title. When not provided, a title will be generated.
This property will be used instead of the Name property.
- `pr-title` (string?): Pull Request title. When not provided, the title will be defined based on the branch name.
Title will be the last part of the branchname split on `/`, so `feature/123-testBranch` will result in title `123-testBranch`
- `to-branch` (string): Name of the branch the pull request should be merged into. For instance `develop`, or `main`.
- `reviewer-ids` (System.Collections.Generic.List<string>): List of reviewer ids. The id should be a valid Azure DevOps user id (ie. GUID).
- `draft-pr` (bool): Boolean specifying if th PR should be marked as draft.
- `include-work-items` (bool): Boolean specifying if workitems should be included in the PR. The workitems will be found by using the commit messages.
- `open-in-browser` (bool): Boolean specifying if the Pull request should be opened in the browser after creation.
- `auto-complete` (RepoM.Plugin.AzureDevOps.ActionProvider.Options.RepositoryActionAzureDevOpsCreatePullRequestsAutoCompleteOptionsV1): Auto complete options. Please take a look at the same for more information
[## `azure-devops-get-prs@1`](#azure-devops-get-prs@1)

This action results in zero or more items in the contextmenu. For each open pullrequest for the given repository, it will show an action to go to the specific PullRequest in your favorite webbrowser.

Action specific properties:
- `project-id` (string?): The azure devops project id.
- `repository-id` (string?): The repository Id. If not provided, the repository id is located using the remote url.
- `show-when-empty` (string?): When no pull requests are available, this property is used to determine if no or a message item is showed.
