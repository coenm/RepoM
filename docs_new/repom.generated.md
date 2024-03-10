# RepoM Core Repository Actions

This module contains the following methods, variables and/or constants:

## browse-repository@1

Action to open the default webbrowser and go to the origin remote webinterface. When multiple remotes are available a sub menu is created for each remote.

Properties:

- `name`: Name of the menu item. ([Text](docs_new/repository_action_types.md#text))
- `active`: Whether the menu item is enabled. ([Predicate](docs_new/repository_action_types.md#predicate))
- `first-only`: Single menu for the first remote. ([Predicate](docs_new/repository_action_types.md#predicate))

## command@1

Action to excute a command (related to the repository)

Properties:

- `name`: Name of the menu item. ([Text](docs_new/repository_action_types.md#text))
- `active`: Whether the menu item is enabled. ([Predicate](docs_new/repository_action_types.md#predicate))
- `command`: The command to execute. ([Text](docs_new/repository_action_types.md#text))
- `arguments`: Arguments for the command. ([Text](docs_new/repository_action_types.md#text))

## executable@1

Action to excute an application with additional arguments. This action is almost identical to the `command@1` action. When no existing executables are provided, the action will not show.

Properties:

- `name`: Name of the menu item. ([Text](docs_new/repository_action_types.md#text))
- `active`: Whether the menu item is enabled. ([Predicate](docs_new/repository_action_types.md#predicate))
- `executable`: The executable. ([Text](docs_new/repository_action_types.md#text))
- `arguments`: Arguments for the executable. ([Text](docs_new/repository_action_types.md#text))
- `context`: The context in which the action is available. ([Context](docs_new/repository_action_types.md#context))

## folder@1

Action to create a folder (sub menu) in the context menu of the repository allowing you to order actions.

Properties:

- `actions`: List of actions. (ActionMenu, optional)
- `name`: Name of the menu item. ([Text](docs_new/repository_action_types.md#text))
- `active`: Whether the menu item is enabled. ([Predicate](docs_new/repository_action_types.md#predicate))
- `context`: The context in which the action is available. ([Context](docs_new/repository_action_types.md#context))

## foreach@1

Action to create repeated actions based on a variable.

Properties:

- `active`: Whether the menu item is enabled. ([Predicate](docs_new/repository_action_types.md#predicate))
- `context`: The context in which the action is available. ([Context](docs_new/repository_action_types.md#context))
- `enumerable`: The list of items to enumerate on. (Variable)
- `variable`: The name of the variable to access to current enumeration of the  items. For each iteration, the variable `{var.name}` has the value of the current iteration. (string?, optional)
- `skip`: Predicate to skip the current item. ([Predicate](docs_new/repository_action_types.md#predicate))
- `actions`: List of repeated actions. (List)

## git-checkout@1

This action will create a menu and sub menus with all local and remote branches for an easy checkout.

Properties:

- `name`: Name of the menu item. ([Text](docs_new/repository_action_types.md#text))
- `active`: Whether the menu item is enabled. ([Predicate](docs_new/repository_action_types.md#predicate))

## git-fetch@1

Action to execute a `git fetch` command.

Properties:

- `name`: Name of the menu item. ([Text](docs_new/repository_action_types.md#text))
- `active`: Whether the menu item is enabled. ([Predicate](docs_new/repository_action_types.md#predicate))

## git-pull@1

Action to execute a `git pull` command.

Properties:

- `name`: Name of the menu item. ([Text](docs_new/repository_action_types.md#text))
- `active`: Whether the menu item is enabled. ([Predicate](docs_new/repository_action_types.md#predicate))

## git-push@1

Action to execute a `git push` command.

Properties:

- `name`: Name of the menu item. ([Text](docs_new/repository_action_types.md#text))
- `active`: Whether the menu item is enabled. ([Predicate](docs_new/repository_action_types.md#predicate))

## ignore-repository@1

Action to ignore the current repository. This repository will be added to the list of ignored repositories and will never show in RepoM.
To undo this action, clear all ignored repositories or manually edit the ignored repositories file (when RepoM is not running).

Properties:

- `name`: Name of the menu item. ([Text](docs_new/repository_action_types.md#text))
- `active`: Whether the menu item is enabled. ([Predicate](docs_new/repository_action_types.md#predicate))
- `context`: The context in which the action is available. ([Context](docs_new/repository_action_types.md#context))

## just-text@1

Textual action to display some text in the action menu.

Properties:

- `name`: Name of the menu item. ([Text](docs_new/repository_action_types.md#text))
- `active`: Whether the menu item is enabled. ([Predicate](docs_new/repository_action_types.md#predicate))
- `enabled`: Show the menu as enabled (clickable) or disabled. ([Predicate](docs_new/repository_action_types.md#predicate))
- `context`: The context in which the action is available. ([Context](docs_new/repository_action_types.md#context))

## pin-repository@1

Action to pin (or unpin) the current repository. Pinning is not persistant and all pinned repositories will be cleared when RepoM exits.
Pinning a repository allowed custom filtering, ordering and searching.

Properties:

- `name`: Name of the menu item. ([Text](docs_new/repository_action_types.md#text))
- `active`: Whether the menu item is enabled. ([Predicate](docs_new/repository_action_types.md#predicate))
- `context`: The context in which the action is available. ([Context](docs_new/repository_action_types.md#context))
- `mode`: The pin mode `[Toggle, Pin, UnPin]`. (Nullable, optional)

## separator@1

Creates a visual separator in the action menu.

Properties:

- `active`: Whether the menu item is enabled. ([Predicate](docs_new/repository_action_types.md#predicate))
- `context`: The context in which the action is available. ([Context](docs_new/repository_action_types.md#context))

## url@1

Action to open the url in the default browser.

Properties:

- `name`: Name of the menu item. ([Text](docs_new/repository_action_types.md#text))
- `url`: The URL to browse to. ([Text](docs_new/repository_action_types.md#text))
- `active`: Whether the menu item is enabled. ([Predicate](docs_new/repository_action_types.md#predicate))
- `context`: The context in which the action is available. ([Context](docs_new/repository_action_types.md#context))
