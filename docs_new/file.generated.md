


In order to use the functions provided by this module, you need to import this module:

```kalk
>>> import File
```

## dir_exists

`dir_exists(path)`

Checks if the specified directory path exists on the disk.

- `path`: Absolute path to a directory.

### Returns

`true` if the specified directory path exists on the disk, `false` otherwise.

### Example

```kalk
dir_exists "testdir"
# dir_exists("testdir")
out = true
 rmdir "testdir"
 dir_exists "testdir"
# dir_exists("testdir")
out = false
```


## file_exists

`file_exists(path)`

Checks if the specified file path exists on the disk.

- `path`: Absolute path to a file.

### Returns

`true` if the specified file path exists on the disk, `false` otherwise.

### Example

```kalk
 rm "test.txt"
 file_exists "test.txt"
# file_exists("test.txt")
out = false
 save_text("content", "test.txt")
 file_exists "test.txt"
# file_exists("test.txt")
out = true
```


## find_files

`find_files(rootPath,searchPattern)`

Find files in a given directory based on the search pattern. Resulting filenames are absolute path based.

- `rootPath`: The root folder.
- `searchPattern`: The search string to match against the names of directories. This parameter can contain a combination of valid literal path and wildcard (`*` and `?`) characters, but it doesn't support regular expressions.

### Returns

Returns an enumerable collection of full paths of the files or directories that matches the specified search pattern.

### Example

Locate all solution files in the given directory.

#### Input
```yaml
find_files 'C:\Users\coenm\RepoM' '*.sln'
# find_files('C:\Users\coenm\RepoM','*.sln')
```

#### Result

```yaml
["C:\Users\coenm\RepoM\src\RepoM.sln"]
```
