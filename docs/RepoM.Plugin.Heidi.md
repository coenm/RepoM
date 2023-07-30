# Heidi

*todo description*

To use this module, make sure it is enabled in RepoM by opening the menu and navigate to 'Plugins'. When enabling or disabling a plugin, you should restart RepoM. <!-- singleLineInclude: _plugin_enable. path: /docs/mdsource/_plugin_enable.include.md -->

## Configuration <!-- include: DocsModuleSettingsTests.DocsModuleSettings_HeidiPackage#desc.verified.md -->

This plugin has specific configuration stored in a separate configuration file stored in `%APPDATA%/RepoM/Module/` directory. This configuration file should be edit manually. The safest way to do this is, is when RepoM is not running.

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

Properties:

- `ConfigPath`: The full directory where the configuration is stored.
- `ConfigFilename`: The configurration filename (without path)
- `ExecutableFilename`: The full executable of Heidi. <!-- endInclude -->

## heidi-databases@1 <!-- include: _plugins.heidi.action. path: /docs/mdsource/_plugins.heidi.action.include.md -->

Action to list heidi databases and show action menues for them.

<!-- todo, improve docs -->

Custom properties:

- Name: The name of the item (required, string, evaulated)
- Enabled: If the action is clickable (optional, boolean/string, evaluated, default true)
- Executable: The absolute path of the Heidi executable (optional, string, evaluated)

Example:

<!-- snippet: RepositoryActionsHeidiDatabases01 -->
<a id='snippet-repositoryactionsheididatabases01'></a>
```yaml
repository-actions:
  actions:
  - type: heidi-databases@1
    active: true
    variables: []
    name: Databases
    executable: ''

  - type: heidi-databases@1
    name: Databases
    executable: ''

  - type: heidi-databases@1
    name: Databases
```
<sup><a href='/tests/RepoM.Plugin.Heidi.Tests/DocumentationFiles/HeidiDatabases.testfile.yaml#L3-L20' title='Snippet source file'>snippet source</a> | <a href='#snippet-repositoryactionsheididatabases01' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->
<!-- endInclude -->
