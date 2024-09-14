
In order to let RepoM know what database configuration should be linked to what git repository the following should be added in the comment section when editing a database configuration in the Heidi's session manager.

- `repo`: Selector of the git repository (required, string)
- `name`: Name of the title in the RepoM menu (required, string)
- `order`: Order of appearance in RepoM. This is only applicable when multiple databases are linked to the same repository (optional, integer, default `0`)

Example:

```text
#repo:RepoM
#name:"RepoM Test database" 
#order:31
```

![Screenshot](HeidiSQL.png)
![Screenshot](HeidiInRepoM.png)