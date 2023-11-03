# `sonarcloud`

Provides a sonar cloud method providing the favorite status of the current repository.

This module contains the following methods, variables and/or constants:

- [`sonarcloud.is_favorite`](#sonarcloud-is-favorite)

## is_favorite

`sonarcloud.is_favorite(id)`

Get favorite status of repository related to the id.

Argument:

- `id`: The sonarcloud id related to the repository.

### Returns

`true` when the repository is set as favorite in SonarCloud, `false`, otherwise.

### Example
      

```
sonarcloud_repository_id = "RepoM";
is_favorite = sonarcloud.is_favorite(sonarcloud_repository_id);
```

