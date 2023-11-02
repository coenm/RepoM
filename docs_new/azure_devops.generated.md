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

Returns an enumeration of pull requests for the selected repository (or an empty enumeration when no pull requests are found).

### Example

Get all pull requests for the selected repository in a given devops project:

#### Usage

```yaml
devops_project_id = "805ACF64-0F06-47EC-96BF-E830895E2740";
prs = azure_devops.get_pull_requests(devops_project_id);
```

#### Result

As a result, the variable `prs` could contain two pull requests with the following dummy data:

```yaml
- repository-id: b1a0619a-cb69-4bf6-9b97-6c62481d9bff
  name: some pr1
  url: https://my-url/pr1
- repository-id: f99e85ee-2c23-414b-8804-6a6c34f8c349
  name: other pr - bug
  url: https://my-url/pr3
```
