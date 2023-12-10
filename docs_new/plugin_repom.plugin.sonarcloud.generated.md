# SonarCloud

This module integrates with SonarCloud. Currently, the only functionality is to star a given repository in SonarCloud using the repository action.

To use this module, make sure it is enabled in RepoM by opening the menu and navigate to 'Plugins'. When enabling or disabling a plugin, you should restart RepoM.

ProjectName: RepoM.Plugin.SonarCloud
PluginName: SonarCloud
PluginDescription: Providing a repository action to mark a repository as favorite in SonarCloud
PluginMarkdownDescription: This module integrates with SonarCloud. Currently, the only functionality is to star a given repository in SonarCloud using the repository action.

This module contains the following methods, variables and/or constants:

[## `sonarcloud-set-favorite@1`](#sonarcloud-set-favorite@1)

Action to mark a repository as favorite within SonarCloud.

Action specific properties:
- `project` (RepoM.ActionMenu.Interface.YamlModel.Templating.Text): The SonarCloud project key.
- `context` (RepoM.ActionMenu.Interface.YamlModel.ActionMenus.Context?): 
- `active` (RepoM.ActionMenu.Interface.YamlModel.Templating.Predicate): 
[## `sonarcloud-set-favorite@1`](#sonarcloud-set-favorite@1)

Action to mark a repository as favorite within SonarCloud.

Action specific properties:
- `project` (string?): The SonarCloud project key.
