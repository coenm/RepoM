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

```yaml
- metadata:
    name: heidi-key
    order: 1
    tags:
    - Test
    - Dev
  database:
    key: MyDomainDb1
    host: database.my-domain.com
    user: coenm
    password: myS3cr3t!
    port: 2345
    uses-windows-authentication: false
    database-type:
      name: MariaDB/MySQL
      protocol: named pipe
    library: MSOLEDBSQL
    comment: HeidiSQL Comment
    databases:
    - database1
    - database2
```

#### RepositoryAction sample


```yaml
context:
- type: evaluate-script@1
  content: |-
    databases = heidi.databases;
    exe_heidi_sql = "C:/Program Files/HeidiSQL/heidisql.exe";    
    exe_ssms ="C:/Program Files (x86)/Microsoft SQL Server Management Studio 18/Common7/IDE/Ssms.exe";

action-menu:
- type: foreach@1
  active: 'array.size(databases) > 0'
  enumerable: databases
  variable: db
  actions:
  # open in Heidi Sql
  - type: executable@1
    name: Open {{ db.metadata.name }} in HeidiSQL
    executable: '{{ exe_heidi_sql }}'
    arguments: --description "{{ db.database.key }}"
  # open in SQL Server Management Studio
  - type: executable@1
    name: Open {{ db.metadata.name }} in SQL Server Management Studio
    executable: '{{ exe_ssms }}'
    arguments: -S "{{ db.database.host }}" -d "{{ array.first db.database.databases }}" -U "{{ db.database.user }}"
```
