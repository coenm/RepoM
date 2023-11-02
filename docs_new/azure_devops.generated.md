# `azure_devops`

Provides Azure Devops functions through `azure_devops`.

This module contains the following methods, variables and/or constants:

- [`azure_devops.get_pull_requests`](#azure-devops-get-pull-requests)

## get_pull_requests

`azure_devops.get_pull_requests(projectId)`

Get pull requests for the given project. The result is an enumeration of .

Argument:

- `projectId`: The azure devops project id. Cannot be null or empty.

### Returns

Returns an enumeration of pull requests or an empty enumeration when no pull requests are found.

### Example

Locate all solution files in the given directory.

#### Input

```yaml
azure_devops.get_pull_requests 'project_id'
# azure_devops.get_pull_requests('project_id')
```

#### Result

```yaml
[ {} {} {}] todo
```
