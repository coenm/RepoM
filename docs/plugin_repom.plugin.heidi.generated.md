# HeidiSQL

This module integrates with a portable [HeidiSQL](https://www.heidisql.com/)  installation. The portable Heidi DB saves its database configuration in a portable configuration file. This module monitors this file and makes it possible to use this configuration in the action menu.

To use this module, make sure it is enabled in RepoM by opening the menu and navigate to 'Plugins'. After enabling or disabling a plugin, you should restart RepoM.

## Configuration

Default configuration:

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


