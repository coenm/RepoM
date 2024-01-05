# RepoM Core Repository Actions

This module contains the following methods, variables and/or constants:

## browse-repository@1

Action to open the default webbrowser and go to the origin remote webinterface. When multiple remotes are available a sub menu is created for each remote.

Properties:

- `name`: Name of the menu item. ([Text](https://this-is.com/Text))
- `active`: Whether the menu item is enabled. ([Predicate](https://this-is.com/Predicate))
- `first-only`: Single menu for the first remote. ([Predicate](https://this-is.com/Predicate))

## command@1

Action to excute a command (related the the repository)

Properties:

- `name`: Name of the menu item. ([Text](https://this-is.com/Text))
- `active`: Whether the menu item is enabled. ([Predicate](https://this-is.com/Predicate))
- `command`: The command to execute. ([Text](https://this-is.com/Text))
- `arguments`: Arguments for the command. ([Text](https://this-is.com/Text))

## executable@1

Action to excute an application with additional arguments. This action is almost identical to the `command@1` action. When no existing executables are provided, the action will not show.

Properties:

- `name`: Name of the menu item. ([Text](https://this-is.com/Text))
- `active`: Whether the menu item is enabled. ([Predicate](https://this-is.com/Predicate))
- `executable`: The executable. ([Text](https://this-is.com/Text))
- `arguments`: Arguments for the executable. ([Text](https://this-is.com/Text))
- `context`: The context in which the action is available. ([Context](https://this-is.com/Context))

## folder@1

Action to create a folder (sub menu) in the context menu of the repository allowing you to order actions.

Properties:

- `actions`: List of actions. (ActionMenu, optional)
- `name`: Name of the menu item. ([Text](https://this-is.com/Text))
- `active`: Whether the menu item is enabled. ([Predicate](https://this-is.com/Predicate))
- `context`: The context in which the action is available. ([Context](https://this-is.com/Context))

## foreach@1

Action to create repeated actions based on a variable.

Properties:

- `active`: Whether the menu item is enabled. ([Predicate](https://this-is.com/Predicate))
- `context`: The context in which the action is available. ([Context](https://this-is.com/Context))
- `enumerable`: The list of items to enumerate on. (Variable)
- `variable`: The name of the variable to access to current enumeration of the  items. For each iteration, the variable `{var.name}` has the value of the current iteration. (string?, optional)
- `skip`: Predicate to skip the current item. ([Predicate](https://this-is.com/Predicate))
- `actions`: List of repeated actions. (List)

## git-checkout@1

This action will create a menu and sub menus with all local and remote branches for an easy checkout.

Properties:

- `name`: Name of the menu item. ([Text](https://this-is.com/Text))
- `active`: Whether the menu item is enabled. ([Predicate](https://this-is.com/Predicate))

## git-fetch@1

Action to execute a `git fetch` command.

Properties:

- `name`: Name of the menu item. ([Text](https://this-is.com/Text))
- `active`: Whether the menu item is enabled. ([Predicate](https://this-is.com/Predicate))

## git-pull@1

Action to execute a `git pull` command.

Properties:

- `name`: Name of the menu item. ([Text](https://this-is.com/Text))
- `active`: Whether the menu item is enabled. ([Predicate](https://this-is.com/Predicate))

## git-push@1

Action to execute a `git push` command.

Properties:

- `name`: Name of the menu item. ([Text](https://this-is.com/Text))
- `active`: Whether the menu item is enabled. ([Predicate](https://this-is.com/Predicate))

## ignore-repository@1

Action to ignore the current repository. This repository will be added to the list of ignored repositories and will never show in RepoM.
To undo this action, clear all ignored repositories or manually edit the ignored repositories file (when RepoM is not running).

Properties:

- `name`: Name of the menu item. ([Text](https://this-is.com/Text))
- `active`: Whether the menu item is enabled. ([Predicate](https://this-is.com/Predicate))
- `context`: The context in which the action is available. ([Context](https://this-is.com/Context))

## just-text@1

Textual action to display some text in the action menu.

Properties:

- `name`: Name of the menu item. ([Text](https://this-is.com/Text))
- `active`: Whether the menu item is enabled. ([Predicate](https://this-is.com/Predicate))
- `enabled`: Show the menu as enabled (clickable) or disabled. ([Predicate](https://this-is.com/Predicate))
- `context`: The context in which the action is available. ([Context](https://this-is.com/Context))

## pin-repository@1

Action to pin (or unpin) the current repository. Pinning is not persistant and all pinned repositories will be cleared when RepoM exits.
Pinning a repository allowed custom filtering, ordering and searching.

Properties:

- `name`: Name of the menu item. ([Text](https://this-is.com/Text))
- `active`: Whether the menu item is enabled. ([Predicate](https://this-is.com/Predicate))
- `context`: The context in which the action is available. ([Context](https://this-is.com/Context))
- `mode`: The pin mode `[Toggle, Pin, UnPin]`. (Nullable, optional)

## separator@1

Creates a visual separator in the action menu.

Properties:

- `active`: Whether the menu item is enabled. ([Predicate](https://this-is.com/Predicate))
- `context`: The context in which the action is available. ([Context](https://this-is.com/Context))

## url@1

Action to open the url in the default browser.

Properties:

- `name`: Name of the menu item. ([Text](https://this-is.com/Text))
- `url`: The URL to browse to. ([Text](https://this-is.com/Text))
- `active`: Whether the menu item is enabled. ([Predicate](https://this-is.com/Predicate))
- `context`: The context in which the action is available. ([Context](https://this-is.com/Context))
