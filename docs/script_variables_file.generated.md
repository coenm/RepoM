# `file`

Provides file related action menu functions and variables accessible through `file`.

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

<!-- snippet: dir_exists@actionmenu01 -->
<a id='snippet-dir_exists@actionmenu01'></a>
```yaml
context:

# create a variable to store the path to the Visual Studio Code executable
- type: evaluate-script@1
  content: |-
    exe_vs_code = env.LocalAppData + "/Programs/Microsoft VS Code/code.exe";

# create a variable to store the path to the documentation directory
# based on the remote name
- type: render-variable@1
  name: repo_docs_directory
  value: 'G:\\My Drive\\RepoDocs\\github.com\\{{ remote_name_origin }}'

action-menu:

# If the document directory exists ..
- type: folder@1
  name: Documentation
  active: file.dir_exists(repo_docs_directory)
  is-deferred: true
  actions:
  # .. show the menu item to open it in Visual Studio Code
  - type: executable@1
    name: Open in Visual Studio Code
    executable: '{{ exe_vs_code }}'
    arguments: '"{{ repo_docs_directory }}"'
  # .. and a menu item to open it in Windows File Explorer
  - type: command@1
    name: Open in Windows File Explorer
    command: '"{{ repo_docs_directory }}"'

# if the directory does not exists, create a menu item to create it
- type: command@1
  name: Create Documentation directory
  command: cmd
  arguments: /k mkdir "{{ repo_docs_directory }}"
  active: '!file.dir_exists(repo_docs_directory)'
```
<sup><a href='/tests/RepoM.ActionMenu.Core.Tests/ActionMenu/IntegrationTests/FilesContextTests.Context_Documentation.DirectoryExists.testfile.yaml#L1-L41' title='Snippet source file'>snippet source</a> | <a href='#snippet-dir_exists@actionmenu01' title='Start of snippet'>anchor</a></sup>
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

<!-- snippet: file_exists@actionmenu01 -->
<a id='snippet-file_exists@actionmenu01'></a>
```yaml
action-menu:
# Show menu item to edit the .editorconfig file if it exists.
- type: executable@1
  name: Edit .editorconfig in Visual Studio Code
  executable: '{{ env.LocalAppData }}/Programs/Microsoft VS Code/code.exe'
  arguments: '"{{ repository.linux_path }}/.editorconfig"'
  active: 'file.file_exists(repository.linux_path + "/.editorconfig")'
```
<sup><a href='/tests/RepoM.ActionMenu.Core.Tests/ActionMenu/IntegrationTests/FilesContextTests.Context_Documentation.FileExists.testfile.yaml#L1-L11' title='Snippet source file'>snippet source</a> | <a href='#snippet-file_exists@actionmenu01' title='Start of snippet'>anchor</a></sup>
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

    solution_files = file.find_files(repository.path, "*.sln");

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
<sup><a href='/tests/RepoM.ActionMenu.Core.Tests/ActionMenu/IntegrationTests/FilesContextTests.Context_Documentation.FindFiles.testfile.yaml#L1-L32' title='Snippet source file'>snippet source</a> | <a href='#snippet-find_files@actionmenu01' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

