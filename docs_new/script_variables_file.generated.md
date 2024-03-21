# `file`

Provides file related action menu functions and variables accessable through `file`.

This module contains the following methods, variables and/or constants:

- [`file.dir_exists`](#dir_exists)
- [`file.file_exists`](#file_exists)
- [`file.find_files`](#find_files)

## dir_exists

`file.dir_exists(path)`

Checks if the specified directory path exists on the disk.

Argument:

- `path`: Absolute path to a directory.

### Returns

`true` if the specified directory path exists on the disk, `false` otherwise.

### Example
      
#### Usage

Check if directory exists


```
exists = file.dir_exists('C:\Project\');
exists = file.dir_exists('C:\Project');
exists = file.dir_exists('C:/Project/');
```

#### RepositoryAction sample

TODO: this content is not correct, change filename

<!-- snippet: find_files@actionmenu01 -->
<a id='snippet-find_files@actionmenu01'></a>
```yaml
context:
- type: evaluate-script@1
  content: |-
    func get_filename(path)
      ret path | string.split("\\") | array.last
    end

    solution_files = file.find_files(repository.safe_path, "*.sln");

action-menu:
# Open in visual studio when only one sln file was found in the repo.
- type: command@1
  name: Open in Visual Studio
  command: '{{ array.first(solution_files) }}'
  active: 'array.size(solution_files) == 1'

# Use folder to choose sln file when multiple sln files found.
- type: folder@1
  name: Open in Visual Studio
  active: 'array.size(solution_files) > 1'
  actions:
  - type: foreach@1
    enumerable: solution_files
    variable: sln
    actions:
    - type: command@1
      name: '{{ get_filename(sln) }}'
      command: '{{ sln }}'
```
<sup><a href='/tests/RepoM.ActionMenu.Core.Tests/ActionMenu/IntegrationTests/FilesContextTests.Context_FindFiles_Documentation.testfile.yaml#L1-L32' title='Snippet source file'>snippet source</a> | <a href='#snippet-find_files@actionmenu01' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->


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

<!-- snippet: find_files@actionmenu01 -->
<a id='snippet-find_files@actionmenu01'></a>
```yaml
context:
- type: evaluate-script@1
  content: |-
    func get_filename(path)
      ret path | string.split("\\") | array.last
    end

    solution_files = file.find_files(repository.safe_path, "*.sln");

action-menu:
# Open in visual studio when only one sln file was found in the repo.
- type: command@1
  name: Open in Visual Studio
  command: '{{ array.first(solution_files) }}'
  active: 'array.size(solution_files) == 1'

# Use folder to choose sln file when multiple sln files found.
- type: folder@1
  name: Open in Visual Studio
  active: 'array.size(solution_files) > 1'
  actions:
  - type: foreach@1
    enumerable: solution_files
    variable: sln
    actions:
    - type: command@1
      name: '{{ get_filename(sln) }}'
      command: '{{ sln }}'
```
<sup><a href='/tests/RepoM.ActionMenu.Core.Tests/ActionMenu/IntegrationTests/FilesContextTests.Context_FindFiles_Documentation.testfile.yaml#L1-L32' title='Snippet source file'>snippet source</a> | <a href='#snippet-find_files@actionmenu01' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->


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

<!-- snippet: find_files@actionmenu01 -->
<a id='snippet-find_files@actionmenu01'></a>
```yaml
context:
- type: evaluate-script@1
  content: |-
    func get_filename(path)
      ret path | string.split("\\") | array.last
    end

    solution_files = file.find_files(repository.safe_path, "*.sln");

action-menu:
# Open in visual studio when only one sln file was found in the repo.
- type: command@1
  name: Open in Visual Studio
  command: '{{ array.first(solution_files) }}'
  active: 'array.size(solution_files) == 1'

# Use folder to choose sln file when multiple sln files found.
- type: folder@1
  name: Open in Visual Studio
  active: 'array.size(solution_files) > 1'
  actions:
  - type: foreach@1
    enumerable: solution_files
    variable: sln
    actions:
    - type: command@1
      name: '{{ get_filename(sln) }}'
      command: '{{ sln }}'
```
<sup><a href='/tests/RepoM.ActionMenu.Core.Tests/ActionMenu/IntegrationTests/FilesContextTests.Context_FindFiles_Documentation.testfile.yaml#L1-L32' title='Snippet source file'>snippet source</a> | <a href='#snippet-find_files@actionmenu01' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

