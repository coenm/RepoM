# HeidiSQL

This module integrates with a portable [HeidiSQL](https://www.heidisql.com/)  installation. The portable Heidi DB saves its database configuration in a portable configuration file. This module monitors this file and makes it possible to use this configuration in the action menu.

To use this module, make sure it is enabled in RepoM by opening the menu and navigate to 'Plugins'. After enabling or disabling a plugin, you should restart RepoM.

## Configuration

This plugin has specific configuration stored in the following directory `%APPDATA%/RepoM/Module/`. This configuration file should be edit manually. The safest way to do this is, is when RepoM is not running.

The following default configuration is used:

```json
{
  "Version": 1,
  "Settings": {
    "ConfigPath": null,
    "ConfigFilename": null,
    "ExecutableFilename": null
  }
}
```

Example configuration:

```json
{
  "Version": 1,
  "Settings": {
    "ConfigPath": "C:\\StandAloneProgramFiles\\HeidiSQL_12.3_64_Portable",
    "ConfigFilename": "portable_settings.txt",
    "ExecutableFilename": "C:\\StandAloneProgramFiles\\HeidiSQL_12.3_64_Portable\\heidisql.exe"
  }
}
```

### Properties

- `ConfigPath`: The full directory where the portable configuration file is stored.
- `ConfigFilename`: The portable-configurration filename (without path). Most likely `portable_settings.txt`
- `ExecutableFilename`: The full executable of Heidi.

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
