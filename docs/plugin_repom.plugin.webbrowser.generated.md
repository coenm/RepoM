# WebBrowser

Provides functionality to start a web browser from an action with profile information.

To use this module, make sure it is enabled in RepoM by opening the menu and navigate to 'Plugins'. After enabling or disabling a plugin, you should restart RepoM.

## Configuration

This plugin has specific configuration stored in the following directory `%APPDATA%/RepoM/Module/`. This configuration file should be edit manually. The safest way to do this is, is when RepoM is not running.

The following default configuration is used:

```json
{
  "Version": 1,
  "Settings": {
    "Browsers": null,
    "Profiles": null
  }
}
```

Example configuration:

```json
{
  "Version": 1,
  "Settings": {
    "Browsers": {
      "Edge": "C:\\PathTo\\msedge.exe",
      "FireFox": "C:\\PathTo\\Mozilla\\firefox.exe"
    },
    "Profiles": {
      "Work": {
        "BrowserName": "Edge",
        "CommandLineArguments": "\"--profile-directory=Profile 4\" {url}"
      },
      "Incognito": {
        "BrowserName": "Edge",
        "CommandLineArguments": "-inprivate"
      },
      "Incognito2": {
        "BrowserName": "FireFox",
        "CommandLineArguments": "-inprivate {url}"
      }
    }
  }
}
```

This module contains the following methods, variables and/or constants:

## browser@1

Action opening a webbrowser with the provided url.

Properties:

- `name`: Name of the menu item. ([Text](repository_action_types.md#text))
- `url`: The url to browse to. ([Text](repository_action_types.md#text))
- `profile`: profile name used to select browser and browser profile ([Text](repository_action_types.md#text))
- `context`: The context in which the action is available. ([Context](repository_action_types.md#context))
- `active`: Whether the menu item is enabled. ([Predicate](repository_action_types.md#predicate))

### Example

<!-- snippet: webbrowser-browser@1-scenario01 -->
<a id='snippet-webbrowser-browser@1-scenario01'></a>
```yaml
- type: browser@1
  name: My Github
  url: https://github.com/coenm
  profile: '{{ my_profile }}'
  active: true
```
<sup><a href='/tests/RepoM.Plugin.WebBrowser.Tests/ActionMenu/IntegrationTests/WebBrowserBrowserV1Tests.BrowserScenario01.testfile.yaml#L8-L16' title='Snippet source file'>snippet source</a> | <a href='#snippet-webbrowser-browser@1-scenario01' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

