# SonarCloud

This module integrates with SonarCloud. Currently, the only functionality is to star a given repository in SonarCloud using the repository action.

To use this module, make sure it is enabled in RepoM by opening the menu and navigate to 'Plugins'. After enabling or disabling a plugin, you should restart RepoM.

- ProjectName: RepoM.Plugin.SonarCloud
- PluginName: SonarCloud
- PluginDescription: Providing a repository action to mark a repository as favorite in SonarCloud
- PluginMarkdownDescription: This module integrates with SonarCloud. Currently, the only functionality is to star a given repository in SonarCloud using the repository action.

## Configuration

The configuration. TODO

This module contains the following methods, variables and/or constants:

## sonarcloud-set-favorite@1

Action to mark a repository as favorite within SonarCloud.

Properties:

- `name`: Name of the menu item. ([Text](repository_action_types.md#text))
- `project`: The SonarCloud project key. ([Text](repository_action_types.md#text))
- `context`: The context in which the action is available. ([Context](repository_action_types.md#context))
- `active`: Whether the menu item is enabled. ([Predicate](repository_action_types.md#predicate))

### Example

snippet: sonarcloud-set-favorite@1-scenario01

