# SonarCloud

This module integrates with SonarCloud. Currently, the only functionality is to star a given repository in SonarCloud using the repository action.

To use this module, make sure it is enabled in RepoM by opening the menu and navigate to 'Plugins'. When enabling or disabling a plugin, you should restart RepoM. <!-- singleLineInclude: _plugin_enable. path: /docs/mdsource/_plugin_enable.include.md -->

## Configuration <!-- include: DocsModuleSettingsTests.DocsModuleSettings_SonarCloudPackage#desc.verified.md -->

This plugin has specific configuration stored in a separate configuration file stored in `%APPDATA%/RepoM/Module/` directory. This configuration file should be edit manually. The safest way to do this is, is when RepoM is not running.

The following default configuration is used:

```json
{
  "Version": 1,
  "Settings": {
    "PersonalAccessToken": null,
    "BaseUrl": "https://sonarcloud.io"
  }
}
```

Properties:

- `PersonalAccessToken`: Personal Access Token to access SonarCloud.
- `BaseUrl`: SonarCloud url. Most likely `https//sonarcloud.io`. <!-- endInclude -->

## sonarcloud-set-favorite@1 <!-- include: _plugins.sonarcloud.action. path: /docs/mdsource/_plugins.sonarcloud.action.include.md -->

Action to mark a repository as favorite within SonarCloud. This action requires the use of the SonarCloud plugin.

Custom properties:

- Name: The name of the item (required, string, evaulated)
- Enabled: If the action is clickable (optional, boolean/string, evaluated, default true)
- Project: The SonarCloud project key (required, string, evaluated)

Example:

<!-- snippet: RepositoryActionsSonarCloudSetFavorite01 -->
<a id='snippet-repositoryactionssonarcloudsetfavorite01'></a>
```yaml
repository-actions:
  actions:
  - type: sonarcloud-set-favorite@1
    active: true 
    variables: []
    name: Star repository on SonarCloud
    project: ''
    
  - type: sonarcloud-set-favorite@1
    name: Star repository on SonarCloud
    project: ''
```
<sup><a href='/tests/RepoM.Plugin.SonarCloud.Tests/DocumentationFiles/SonarCloudSetFavorite01.testfile.yaml#L3-L16' title='Snippet source file'>snippet source</a> | <a href='#snippet-repositoryactionssonarcloudsetfavorite01' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->
<!-- endInclude -->
