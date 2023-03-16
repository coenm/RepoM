# Actions

The following actions are part of the core of RepoM and can always be used in your RepositoryActions.

## just-text@1

Textual action to display some text in the action menu.

Custom properties:

- Name: The name of the item (required, string, evaulated)
- Enabled: If the action is clickable (optional, boolean/string, evaluated, default true)

Example:

snippet: RepositoryActionsJustText01

## associate-file@1

Action menu for opening files with a given extension. If files within the repository are found matching the extension, a submenu will be created with all matched files.

Custom properties:

- Name: The name of the item (required, string, evaulated)
- Extension: The file extension to look for. This parameter can contain a combination of valid literal path and wildcard (`*` and `?`) characters, but it doesnt support regular expressions.  (required, string non-evaluated)

Example:

snippet: RepositoryActionsAssociateFile01

## browse-repository@1

Action to open the default webbrowser and go to the origin remote webinterface.

No additional properties and assigning variables has no effect.

Example:

snippet: RepositoryActionsBrowseRepository01

## browser@1

Action opening a webbrowser with the provided url.

Custom properties:

- Name: The name of the item (required, string, evaulated)
- Url: The url to browse to (required, string, evaluated)

Example:

snippet: RepositoryActionsBrowser01

## command@1

Action to excute a command (related the the repository).

Custom properties:

- Name: The name of the item (required, string, evaulated)
- Command: The command to execute (required, string, evaluted)
- Arguments: The arguments to add to the command (optional, string, evaluted, default empty string)

Example:

snippet: RepositoryActionsCommand01

## executable@1

Action to excute an application with additional arguments. This action is almost identical to the `command@1` action. When no existing executables are provided, the action will not show.

Custom properties:

- Name: The name of the item (required, string, evaulated)
- Executables: Array of possible executables. The first executable that exists will be used. The paths should absolute. (required, string, evaluted)
- Arguments: The arguments to add to the executable (optional, string, evaluted, default empty string)

When you want to specify exacly one executable, you can replace the required property `Executables` with the following property:

- Executable: Absolute path of the exeuctable to execute (required, string, evaluted)

Example:

snippet: RepositoryActionsExecutable01

## folder@1

Action to create a folder (sub menu) in the context menu of the repository allowing you to order actions.

Custom properties:

- Name: The name of the item (required, string, evaulated)
- Items: Array of subitems (required, array of actions)

Example:

snippet: RepositoryActionsFolder01

## foreach@1

Action to create repeated actions based on a variable.

Custom properties:

- Enumerable: name of the variable to enumerate over (required, string).
- Variable: variable name of the iteration. For each iteration, the variable `{var.name}` has the value of the current iteration (required, string, non-evaluated)
- Skip: Predicate to skip the current item (optional, string, evaluted, default empty)
- Actions: Array of repeated actions (required, array of actions)

Example:

snippet: RepositoryActionsForeach01

## git-checkout@1

This action will create a menu and sub menus with all local and remote branches for an easy checkout.

No additional properties and assigning variables has no effect.

Example:

snippet: RepositoryActionsGitCheckout01

## git-fetch@1

Action to execute a `git fetch` command.

No additional properties and assigning variables has no effect.

Example:

snippet: RepositoryActionsGitFetch01

## git-pull@1

Action to execute a `git pull` command.

No additional properties and assigning variables has no effect.

Example:

snippet: RepositoryActionsGitPull01

## git-push@1

Action to execute a `git push` command.

No additional properties and assigning variables has no effect.

Example:

snippet: RepositoryActionsGitPush01

## ignore-repository@1

Action to ignore the current repository. This repository will be added to the list of ignored repositories and will never show in RepoM.
To undo this action, clear all ignored repositories or manually edit the ignored repositories file (when RepoM is not running).

No additional properties and assigning variables has no effect.

Example:

snippet: RepositoryActionsIgnoreRepository01

## pin-repository@1

Action to pin (or unpin) the current repository. Pinning is not persistant and all pinned repositories will be cleared when RepoM exits.
Pinning a repository allowed custom filtering, ordering and searching.

Custom properties:

- Name: The name of the item. When not set, default text is used based on the mode (optional, string, evaulated, default empty)
- Mode: Enum `[Toggle, Pin, Unpin]` (required)

Example:

snippet: RepositoryActionsPinRepository01

## separator@1

Creates a visual separator in the action menu.

No additional properties and assigning variables has no effect.

Example:

snippet: RepositoryActionsSeparator01

# Plugin actions

These actions are available though the use of plugins.

include: _plugins.clipboard.action

See the [Clipboard](RepoM.Plugin.Clipboard.md) plugin for more information.

include: _plugins.sonarcloud.action

See the [SonarCloud](RepoM.Plugin.SonarCloud.md) plugin for more information.

## azure-devops-get-prs@1

This action results in zero or more items in the contextmenu. For each open pullrequest for the given repository, it will show an action to go to the specific PullRequest in your favorite webbrowser.
The AzureDevOps plugin is required.

See the [AzureDevOps](RepoM.Plugin.AzureDevOps.md) plugin for more information.


include: _plugins.heidi.action

See the [Heidi](RepoM.Plugin.Heidi.md) plugin for more information.

# Repository Actions

These actions are part of the Repository Actions config file described in [Repository Actions](RepositoryActions.md).
