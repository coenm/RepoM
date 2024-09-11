# Statistics

Provides functionality to keep track how may times an action is performed on a given repository. These numbers can be accessed using variable providers. The plugin also contains functionality to use these statistics in orderings.

To use this module, make sure it is enabled in RepoM by opening the menu and navigate to 'Plugins'. After enabling or disabling a plugin, you should restart RepoM.

## Configuration

This plugin has specific configuration stored in the following directory `%APPDATA%/RepoM/Module/`. This configuration file should be edit manually. The safest way to do this is, is when RepoM is not running.

The following default configuration is used:

```json
{
  "Version": 1,
  "Settings": {
    "PersistenceBuffer": "00:05:00",
    "RetentionDays": 30
  }
}
```


