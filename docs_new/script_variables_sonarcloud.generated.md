# `sonarcloud`

Provides a sonar cloud method providing the favorite status of the current repository.

This module contains the following methods, variables and/or constants:

- [`sonarcloud.is_favorite`](#is_favorite)

## is_favorite

`sonarcloud.is_favorite(id)`

Get favorite status of repository related to the id.

Argument:

- `id`: The sonarcloud id related to the repository.

### Returns

`true` when the repository is set as favorite in SonarCloud, `false`, otherwise.

### Example
      
#### Usage

Gets SonarClouds favorite status of the repository:


```
sonarcloud_repository_id = "RepoM";
is_favorite = sonarcloud.is_favorite(sonarcloud_repository_id);
```

#### Result

As a result, the boolean variable `is_favorite` is set.

#### RepositoryAction sample

<!-- snippet: sonarcloud-is_favorite@actionmenu01 -->
<a id='snippet-sonarcloud-is_favorite@actionmenu01'></a>
```yaml
context:
- type: evaluate-script@1
  content: |-
    sonarcloud_repository_id = "RepoM";
    is_favorite = sonarcloud.is_favorite(sonarcloud_repository_id);

action-menu:
- type: url@1
  name: 'Open SonarClouds favorite in browser'
  url: 'https://sonarcloud.io/project/overview?id={{ sonarcloud_repository_id }}'
  active: is_favorite
```
<sup><a href='/tests/RepoM.Plugin.SonarCloud.Tests/ActionMenu/IntegrationTests/SonarCloudContextTests.Context_IsFavorite_Documentation.testfile.yaml#L1-L15' title='Snippet source file'>snippet source</a> | <a href='#snippet-sonarcloud-is_favorite@actionmenu01' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

