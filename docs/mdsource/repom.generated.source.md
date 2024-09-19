# RepoM Core Repository Actions

The following actions are part of the core of RepoM and can always be used in your RepositoryActions.

- [`browse-repository@1`](#browse-repository@1)
- [`command@1`](#command@1)
- [`executable@1`](#executable@1)
- [`folder@1`](#folder@1)
- [`foreach@1`](#foreach@1)
- [`git-checkout@1`](#git-checkout@1)
- [`git-fetch@1`](#git-fetch@1)
- [`git-pull@1`](#git-pull@1)
- [`git-push@1`](#git-push@1)
- [`ignore-repository@1`](#ignore-repository@1)
- [`just-text@1`](#just-text@1)
- [`pin-repository@1`](#pin-repository@1)
- [`separator@1`](#separator@1)
- [`url@1`](#url@1)

## browse-repository@1

Action to open the default webbrowser and go to the origin remote webinterface. When multiple remotes are available a sub menu is created for each remote.

Properties:

- `name`: Name of the menu item. ([Text](repository_action_types.md#text))
- `active`: Whether the menu item is enabled. ([Predicate](repository_action_types.md#predicate))
- `first-only`: Single menu for the first remote. ([Predicate](repository_action_types.md#predicate))

### Example

snippet: browse-repository@1-scenario01


## command@1

Action to excute a command (related to the repository)

Properties:

- `name`: Name of the menu item. ([Text](repository_action_types.md#text))
- `active`: Whether the menu item is enabled. ([Predicate](repository_action_types.md#predicate))
- `command`: The command to execute. ([Text](repository_action_types.md#text))
- `arguments`: Arguments for the command. ([Text](repository_action_types.md#text))

### Example

snippet: command@1-scenario01


## executable@1

Action to excute an application with additional arguments. This action is almost identical to the `command@1` action. When no existing executables are provided, the action will not show.

Properties:

- `name`: Name of the menu item. ([Text](repository_action_types.md#text))
- `active`: Whether the menu item is enabled. ([Predicate](repository_action_types.md#predicate))
- `executable`: The executable. ([Text](repository_action_types.md#text))
- `arguments`: Arguments for the executable. ([Text](repository_action_types.md#text))
- `context`: The context in which the action is available. ([Context](repository_action_types.md#context))

### Example

snippet: executable@1-scenario01


## folder@1

Action to create a folder (sub menu) in the context menu of the repository allowing you to order actions.

Properties:

- `actions`: List of actions. (ActionMenu, optional)
- `name`: Name of the menu item. ([Text](repository_action_types.md#text))
- `active`: Whether the menu item is enabled. ([Predicate](repository_action_types.md#predicate))
- `context`: The context in which the action is available. ([Context](repository_action_types.md#context))

### Example

snippet: folder@1-scenario01


## foreach@1

Action to create repeated actions based on a variable.

Properties:

- `active`: Whether the menu item is enabled. ([Predicate](repository_action_types.md#predicate))
- `context`: The context in which the action is available. ([Context](repository_action_types.md#context))
- `iteration-context`: Additional context added for each iteration. ([Context](repository_action_types.md#context))
- `enumerable`: The list of items to enumerate on. (Variable)
- `variable`: The name of the variable to access to current enumeration of the  items. For each iteration, the variable `{var.name}` has the value of the current iteration. (string?, optional)
- `skip`: Predicate to skip the current item. ([Predicate](repository_action_types.md#predicate))
- `actions`: List of repeated actions. (List)

### Example

snippet: foreach@1-scenario01

snippet: foreach@1-scenario02


## git-checkout@1

This action will create a menu and sub menus with all local and remote branches for an easy checkout.

Properties:

- `name`: Name of the menu item. ([Text](repository_action_types.md#text))
- `active`: Whether the menu item is enabled. ([Predicate](repository_action_types.md#predicate))

### Example

snippet: git-checkout@1-scenario01


## git-fetch@1

Action to execute a `git fetch` command.

Properties:

- `name`: Name of the menu item. ([Text](repository_action_types.md#text))
- `active`: Whether the menu item is enabled. ([Predicate](repository_action_types.md#predicate))

### Example

snippet: git-fetch@1-scenario01


## git-pull@1

Action to execute a `git pull` command.

Properties:

- `name`: Name of the menu item. ([Text](repository_action_types.md#text))
- `active`: Whether the menu item is enabled. ([Predicate](repository_action_types.md#predicate))

### Example

snippet: git-pull@1-scenario01


## git-push@1

Action to execute a `git push` command.

Properties:

- `name`: Name of the menu item. ([Text](repository_action_types.md#text))
- `active`: Whether the menu item is enabled. ([Predicate](repository_action_types.md#predicate))

### Example

snippet: git-push@1-scenario01


## ignore-repository@1

Action to ignore the current repository. This repository will be added to the list of ignored repositories and will never show in RepoM.
To undo this action, clear all ignored repositories or manually edit the ignored repositories file (when RepoM is not running).

Properties:

- `name`: Name of the menu item. ([Text](repository_action_types.md#text))
- `active`: Whether the menu item is enabled. ([Predicate](repository_action_types.md#predicate))
- `context`: The context in which the action is available. ([Context](repository_action_types.md#context))

### Example

snippet: ignore-repository@1-scenario01


## just-text@1

Textual action to display some text in the action menu.

Properties:

- `name`: Name of the menu item. ([Text](repository_action_types.md#text))
- `active`: Whether the menu item is enabled. ([Predicate](repository_action_types.md#predicate))
- `enabled`: Show the menu as enabled (clickable) or disabled. ([Predicate](repository_action_types.md#predicate))
- `context`: The context in which the action is available. ([Context](repository_action_types.md#context))

### Example

snippet: just-text@1-scenario01


## pin-repository@1

Action to pin (or unpin) the current repository. Pinning is not persistant and all pinned repositories will be cleared when RepoM exits.
Pinning a repository allowed custom filtering, ordering and searching.

Properties:

- `name`: Name of the menu item. ([Text](repository_action_types.md#text))
- `active`: Whether the menu item is enabled. ([Predicate](repository_action_types.md#predicate))
- `context`: The context in which the action is available. ([Context](repository_action_types.md#context))
- `mode`: The pin mode `[Toggle, Pin, UnPin]`. (Nullable, optional)

### Example

snippet: pin-repository@1-scenario01


## separator@1

Creates a visual separator in the action menu.

Properties:

- `active`: Whether the menu item is enabled. ([Predicate](repository_action_types.md#predicate))
- `context`: The context in which the action is available. ([Context](repository_action_types.md#context))

### Example

snippet: separator@1-scenario01


## url@1

Action to open the url in the default browser.

Properties:

- `name`: Name of the menu item. ([Text](repository_action_types.md#text))
- `url`: The URL to browse to. ([Text](repository_action_types.md#text))
- `active`: Whether the menu item is enabled. ([Predicate](repository_action_types.md#predicate))
- `context`: The context in which the action is available. ([Context](repository_action_types.md#context))

### Example

snippet: url@1-scenario01

