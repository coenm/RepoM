# Statistics

This module add functionality to keep track of actions performed on repositories. This can be used in orderings (and mabye later on in filtering) of repositories.

To use this module, make sure it is enabled in RepoM by opening the menu and navigate to 'Plugins'. When enabling or disabling a plugin, you should restart RepoM. <!-- singleLineInclude: _plugin_enable. path: /docs/mdsource/_plugin_enable.include.md -->

This plugin has specific configuration stored in a separate configuration file stored in `%APPDATA%/RepoM/Module/` directory. This configuration file should be edit manually. The safest way to do this is, is when RepoM is not running. <!-- include: DocsModuleSettingsTests.DocsModuleSettings_StatisticsPackage#desc.verified.md -->

The following default configuration is used

```json
{
  "Version": 1,
  "Settings": {
    "PersistenceBuffer": "00:05:00",
    "RetentionDays": 30
  }
}
```
<!-- endInclude -->
