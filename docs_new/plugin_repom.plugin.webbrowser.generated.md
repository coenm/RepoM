# WebBrowser

Provides functionality to start a web browser from an action with profile information.

To use this module, make sure it is enabled in RepoM by opening the menu and navigate to 'Plugins'. When enabling or disabling a plugin, you should restart RepoM.

- ProjectName: RepoM.Plugin.WebBrowser
- PluginName: WebBrowser
- PluginDescription: Provides functionality to start a web browser from an action with profile information.
- PluginMarkdownDescription: \<empty\>

This module contains the following methods, variables and/or constants:

## browser@1

Action opening a webbrowser with the provided url.

Properties:

- `url`: The url to browse to. ([Text](docs_new/repository_action_types.md#text))
- `profile`: profile name used to select browser and browser profile ([Text](docs_new/repository_action_types.md#text))
- `context`: The context in which the action is available. ([Context](docs_new/repository_action_types.md#context))
- `active`: Whether the menu item is enabled. ([Predicate](docs_new/repository_action_types.md#predicate))

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

