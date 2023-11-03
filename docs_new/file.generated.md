# `file`

Provides file related action menu functions and variables accessable through `file`.

This module contains the following methods, variables and/or constants:

- [`file.dir_exists`](#file-dir-exists)
- [`file.file_exists`](#file-file-exists)
- [`file.find_files`](#file-find-files)

## dir_exists

`file.dir_exists(path)`

Checks if the specified directory path exists on the disk.

Argument:

- `path`: Absolute path to a directory.

### Returns

`true` if the specified directory path exists on the disk, `false` otherwise.

### Example
      
#### Usage

Check if file exists

```
exists = file.dir_exists('C:\Project\');
exists = file.dir_exists('C:\Project');
exists = file.dir_exists('C:/Project/');
```

#### RepositoryAction sample

TODO: this content is not correct, change filename

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


## file_exists

`file.file_exists(path)`

Checks if the specified file path exists on the disk.

Argument:

- `path`: Absolute path to a file.

### Returns

`true` if the specified file path exists on the disk, `false` otherwise.

### Example
      
#### Usage

Check if file exists

```
exists = file.file_exists('C:\Project\my-solution.sln');
```

#### RepositoryAction sample

TODO: this content is not correct, change filename

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


## find_files

`file.find_files(rootPath,searchPattern)`

Find files in a given directory based on the search pattern. Resulting filenames are absolute path based.

Arguments:

- `rootPath`: The root folder.
- `searchPattern`: The search string to match against the names of directories. This parameter can contain a combination of valid literal path and wildcard (`*` and `?`) characters, but it doesn't support regular expressions.

### Returns

Returns an enumerable collection of full paths of the files or directories that matches the specified search pattern.

### Example
      
#### Usage

Locate all solution files in the given directory.

```
solution_files = file.find_files('C:\Project\', '*.sln');
```

#### Result

As a result, the variable `solution_files` is an enumerable of strings, for example:

```yaml
- C:\Project\My Repositories\my-solution.sln
- C:\Project\My Repositories\src\test solution.sln
```

#### RepositoryAction sample

TODO: this content is not correct

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

