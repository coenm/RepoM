# WebBrowser

Provides functionality to start a web browser from an action with profile information.

To use this module, make sure it is enabled in RepoM by opening the menu and navigate to 'Plugins'. When enabling or disabling a plugin, you should restart RepoM.

ProjectName: RepoM.Plugin.WebBrowser
PluginName: WebBrowser
PluginDescription: Provides functionality to start a web browser from an action with profile information.
PluginMarkdownDescription: 

This module contains the following methods, variables and/or constants:

[## `browser@1`](#browser@1)

Action opening a webbrowser with the provided url.

Action specific properties:
- `url` (RepoM.ActionMenu.Interface.YamlModel.Templating.Text): The url to browse to.
- `profile` (RepoM.ActionMenu.Interface.YamlModel.Templating.Text): profile name used to select browser and browser profile
- `context` (RepoM.ActionMenu.Interface.YamlModel.ActionMenus.Context?): 
- `active` (RepoM.ActionMenu.Interface.YamlModel.Templating.Predicate): 
[## `browser@1`](#browser@1)

Action opening a webbrowser with the provided url.

Action specific properties:
- `url` (string?): The url to browse to.
- `profile` (string?): profile name used to select browser and browser profile
