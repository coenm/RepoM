# `heidi`

Provides variables provided by the Heidi module. The variables are accessable through `heidi`.

This module contains the following methods, variables and/or constants:

- [`heidi.databases`](#databases)

## databases

`heidi.databases`

Gets all known databases configured in the Heidi configuration related to the selected repository.

### Returns

An enumerable of database configuration objects. Such object contains the following members:

- `metadata.name`: name of the current configuration.
- `metadata.order`: integer specifying a given order in Heidi.
- `metadata.tags`: array of strings containing tags. Can be empty.
- `database.key`: the configuration key. THis key can be used to start Heidi and open the database for this configuration.
- `database.host`: the hostname to connect to the database.
- `database.user`: the username used to connect to the database.
- `database.password`: the password used to connect to the database.

### Example
      

```
databases = heidi.databases
```


```

```

