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

snippet: sonarcloud-is_favorite@actionmenu01

