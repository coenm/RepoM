# SonarCloud

This module integrates with SonarCloud. Currently, the only functionality is to star a given repository in SonarCloud using the repository action.

To use this module, make sure it is enabled in RepoM by opening the menu and navigate to 'Plugins'. After enabling or disabling a plugin, you should restart RepoM.

## Configuration

This plugin has specific configuration stored in the following directory `%APPDATA%/RepoM/Module/`. This configuration file should be edit manually. The safest way to do this is, is when RepoM is not running.

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

### Properties

- `PersonalAccessToken`: Personal Access Token to access SonarCloud.
- `BaseUrl`: SonarCloud url. Most likely `https//sonarcloud.io`.

This module contains the following methods, variables and/or constants:

- [`sonarcloud-set-favorite@1`](#sonarcloud-set-favorite@1)

## sonarcloud-set-favorite@1

Action to mark a repository as favorite within SonarCloud.

Properties:

- `name`: Name of the menu item. ([Text](repository_action_types.md#text))
- `project`: The SonarCloud project key. ([Text](repository_action_types.md#text))
- `context`: The context in which the action is available. ([Context](repository_action_types.md#context))
- `active`: Whether the menu item is enabled. ([Predicate](repository_action_types.md#predicate))

### Example

snippet: sonarcloud-set-favorite@1-scenario01

