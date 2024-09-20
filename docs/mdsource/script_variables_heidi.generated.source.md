# `heidi`

Provides variables provided by the Heidi module. The variables are accessable through `heidi`.

This module contains the following methods, variables and/or constants:

- [`heidi.databases`](#databases)

## databases

`heidi.databases`

Gets all known databases configured in the Heidi configuration related to the selected repository.

### Returns

An enumerable of database configuration objects as shown in the example below.

### Example
      
Get all database configurations for the current repository:


```
databases = heidi.databases;
```

#### Result

As a result, the variable `databases` could contain the following dummy database configuration:

snippet: heidi.databases@actionmenu01

#### RepositoryAction sample

snippet: heidi.databases@actionmenu02

