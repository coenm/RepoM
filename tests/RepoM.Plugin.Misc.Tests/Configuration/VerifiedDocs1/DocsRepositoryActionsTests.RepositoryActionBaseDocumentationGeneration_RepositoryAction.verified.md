Properties:

- `type`: RepositoryAction type. Should be a fixed value used to determine the action type. (required, string)
- `name`: Name of the action. This is shown in the UI of RepoM. (required, evaluated, string)
- `active`: Is the action active (ie. visible) or not. (optional, evaluated, boolean, default: `true`)
- `variables`: A set of variables to be availabe within this action. (optional, list`1)
