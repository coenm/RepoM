---
title: azure_devops Module
url: /doc/api/azure_devops/
---



In order to use the functions provided by this module, you need to import this module:

```kalk
>>> import azure_devops
```

## get_pull_requests

`get_pull_requests(projectId)`

Get pull requests for the given project. The result is an enumerabtion of .

- `projectId`: The azure devops project id.

### Returns

Returns an enumeration of pull requests or empty when no pull requests are found.

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
