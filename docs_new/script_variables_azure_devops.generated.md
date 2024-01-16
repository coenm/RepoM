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

<!-- snippet: azure_devops.get_pull_requests -->
<a id='snippet-azure_devops.get_pull_requests'></a>
```yaml
- repository-id: b1a0619a-cb69-4bf6-9b97-6c62481d9bff
  name: some pr1
  url: https://my-url/pr1
- repository-id: f99e85ee-2c23-414b-8804-6a6c34f8c349
  name: other pr - bug
  url: https://my-url/pr3
```
<sup><a href='/tests/RepoM.Plugin.AzureDevOps.Tests/ActionMenu/Context/AzureDevopsVariablesTests.GetPullRequests_Documentation.verified.yaml#L1-L11' title='Snippet source file'>snippet source</a> | <a href='#snippet-azure_devops.get_pull_requests' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

#### RepositoryAction sample

<!-- snippet: azure-devops-get-pull-requests@actionmenu01 -->
<a id='snippet-azure-devops-get-pull-requests@actionmenu01'></a>
```yaml
context:
- type: evaluate-script@1
  content: |-
    devops_project_id = "805ACF64-0F06-47EC-96BF-E830895E2740";
    prs = azure_devops.get_pull_requests(devops_project_id);

action-menu:
- type: foreach@1
  active: 'array.size(prs) > 1'
  enumerable: prs
  variable: pr
  actions:
  - type: url@1
    name: '{{ pr.name }}'
    url: '{{ pr.url }}'
```
<sup><a href='/tests/RepoM.Plugin.AzureDevOps.Tests/ActionMenu/IntegrationTests/AzureDevopsContextTests.Context_GetPullRequests_Documentation.testfile.yaml#L1-L19' title='Snippet source file'>snippet source</a> | <a href='#snippet-azure-devops-get-pull-requests@actionmenu01' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

