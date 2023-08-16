Action menu for opening files with a given extension. If files within the repository are found matching the extension, a submenu will be created with all matched files.

Action specific properties:

- `extension`: The file extension to look for. This parameter can contain a combination of valid literal path and wildcard (`*` and `?`) characters, but it doesnt support regular expressions.
For example `*.sln`. (required, string)
