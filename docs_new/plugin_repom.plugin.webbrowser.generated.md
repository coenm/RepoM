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

- `url`: The url to browse to. ([Text](https://this-is.com/Text))
- `profile`: profile name used to select browser and browser profile ([Text](https://this-is.com/Text))
- `context`: The context in which the action is available. ([Context](https://this-is.com/Context))
- `active`: Whether the menu item is enabled. ([Predicate](https://this-is.com/Predicate))

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

