# WebBrowser

The WebBrowser module provides a repository action to open an URL in a given webbrowser.

To use this module, make sure it is enabled in RepoM by opening the menu and navigate to 'Plugins'. When enabling or disabling a plugin, you should restart RepoM.<!-- singleLineInclude: _plugin_enable. path: /docs/mdsource/_plugin_enable.include.md -->

## Configuration<!-- include: DocsModuleSettingsTests.DocsModuleSettings_WebBrowserPackage#desc.verified.md -->

This plugin has specific configuration stored in a separate configuration file stored in `%APPDATA%/RepoM/Module/` directory. This configuration file should be edit manually. The safest way to do this is, is when RepoM is not running.

The following default configuration is used:

```json
{
  "Version": 1,
  "Settings": {
    "Browsers": {},
    "Profiles": {}
  }
}
```

Properties:

- `Browsers`: Dictionary of known browsers and their path to use for opening urls.
- `Profiles`: Profiles to use. <!-- endInclude -->

## browser@1<!-- include: _plugins.webbrowser.action. path: /docs/mdsource/_plugins.webbrowser.action.include.md -->

Action opening a webbrowser with the provided url.<!-- include: DocsRepositoryActionsTests.DocsRepositoryActionsSettings_RepositoryActionBrowserV1.verified.md -->

Action specific properties:

- `url`: The url to browse to. (required, evaluated, string)
- `profile`: profile name used to select browser and browser profile (optional, evaluated, string)<!-- endInclude -->

Example:

<!-- snippet: RepositoryActionsBrowser01 -->
<a id='snippet-repositoryactionsbrowser01'></a>
```yaml
repository-actions:
  actions:
  - type: browser@1
    active: true 
    variables: []
    name: 'My Github'
    url: 'https://github.com/coenm'

  - type: browser@1
    name: 'My Github'
    url: 'https://github.com/coenm'
    profile: edge
```
<sup><a href='/tests/RepoM.Plugin.WebBrowser.Tests/DocumentationFiles/Browser01.testfile.yaml#L3-L18' title='Snippet source file'>snippet source</a> | <a href='#snippet-repositoryactionsbrowser01' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->
<!-- endInclude -->
