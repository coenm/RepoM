# `repository`

Provides action menu functions and variables for the current repository through `repository`.

This module contains the following methods, variables and/or constants:

- [`repository.branch`](#branch)
- [`repository.branches`](#branches)
- [`repository.linux_path`](#linux_path)
- [`repository.local_branches`](#local_branches)
- [`repository.location`](#location)
- [`repository.name`](#name)
- [`repository.path`](#path)
- [`repository.remotes`](#remotes)
- [`repository.windows_path`](#windows_path)

## branch

`repository.branch`

Gets the current branch of the repository

### Returns

The name of the current branch.

## branches

`repository.branches`

Gets the current branch of the repository

### Returns

The name of the current branch.

## linux_path

`repository.linux_path`

Gets the path of the repository in linux style (i.e. use `\`). The path does NOT end with a backslash.

### Returns

The backslash based path of the repository without the last backslash.

## local_branches

`repository.local_branches`

Gets the local branches

### Returns

All local branches.

## location

`repository.location`

Gets the Location of the repository.

### Returns

The path of the repository.

## name

`repository.name`

Gets the name of the repository.

### Returns

The name of the repository.

### Example
      
#### Usage


```
repository.name
```


## path

`repository.path`

Gets the path of the repository. The path is windows or linux based (depending on the running OS) and does NOT end with a (back)slash.

### Returns

The repository path.

## remotes

`repository.remotes`

Gets the remotes.

### Returns

Remotes.

## windows_path

`repository.windows_path`

Gets the path of the repository in windows style (i.e. use `/`). The path does NOT end with a slash.

### Returns

The path of the repository.
