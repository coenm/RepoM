# `azure_devops`

Provides Azure Devops functions through `azure_devops`.

This module contains the following methods, variables and/or constants:

- [`azure_devops.get_pull_requests`](#get_pull_requests)

## get_pull_requests

`azure_devops.get_pull_requests(projectId)`

Get pull requests for the given project. The result is an enumeration of PullRequest.

Argument:

- `projectId`: The azure devops project id. Cannot be null or empty.

### Returns

Returns an enumeration of pull requests for the selected repository (or an empty enumeration when no pull requests are found).

### Example
      
#### Usage

Get all pull requests for the selected repository in a given devops project:


```
devops_project_id = "805ACF64-0F06-47EC-96BF-E830895E2740";
prs = azure_devops.get_pull_requests(devops_project_id);
```

#### Result

As a result, the variable `prs` could contain two pull requests with the following dummy data:

snippet: azure_devops.get_pull_requests

#### RepositoryAction sample

snippet: azure-devops-get-pull-requests@actionmenu01

