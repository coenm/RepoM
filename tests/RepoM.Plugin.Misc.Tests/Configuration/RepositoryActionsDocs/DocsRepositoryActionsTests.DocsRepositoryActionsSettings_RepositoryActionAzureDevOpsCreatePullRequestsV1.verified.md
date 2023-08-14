Action menu item to create a pull request in Azure Devops.

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
- `auto-complete`: Auto complete options. Please take a look at the same for more information (required, repositoryactionazuredevopscreatepullrequestsautocompleteoptionsv1)
