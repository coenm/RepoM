# `Repository`

Provides action menu functions and variables for the current repository through `repository`.

## Overview

The following constants and variables are available:
- [`repository.branch`](#repository-branch)
- [`repository.branches`](#repository-branches)
- [`repository.local_branches`](#repository-local-branches)
- [`repository.location`](#repository-location)
- [`repository.name`](#repository-name)
- [`repository.path`](#repository-path)
- [`repository.remotes`](#repository-remotes)
- [`repository.safe_path`](#repository-safe-path)

## Methods

No methods defined.

## Constants and Variables

- [`repository.branch`](#repository-branch)
- [`repository.branches`](#repository-branches)
- [`repository.local_branches`](#repository-local-branches)
- [`repository.location`](#repository-location)
- [`repository.name`](#repository-name)
- [`repository.path`](#repository-path)
- [`repository.remotes`](#repository-remotes)
- [`repository.safe_path`](#repository-safe-path)

## TODO


### branch

`repository.branch`

Gets the current branch of the repository

#### Returns

The name of the current branch.

### branches

`repository.branches`

Gets the current branch of the repository

#### Returns

The name of the current branch.

### local_branches

`repository.local_branches`

Gets the local branches

#### Returns

All local branches.

### location

`repository.location`

Gets the Location of the repository.

#### Returns

The path of the repository.

### name

`repository.name`

Gets the name of the repository.

#### Returns

The name of the repository.

#### Example



##### Input

```yaml
repository.name
```

##### Result

```yaml
"RepoM"
```

### path

`repository.path`

Gets the path of the repository.

#### Returns

The path of the repository.

### remotes

`repository.remotes`

Gets the remotes.

#### Returns

Remotes.

### safe_path

`repository.safe_path`

Gets the safe path of the repository.

#### Returns

The path of the repository.
