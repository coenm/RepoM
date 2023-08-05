# Actions

Most of the repository actions are part of the core of RepoM and some are part of external plugins. All these repository actions have the same base:

include: DocsRepositoryActionsTests.RepositoryActionBaseDocumentationGeneration_RepositoryAction.verified.md

The following actions are part of the core of RepoM and can always be used in your RepositoryActions.

## just-text@1

include: DocsRepositoryActionsTests.DocsRepositoryActionsSettings_RepositoryActionJustTextV1.verified.md

Example:

snippet: RepositoryActionsJustText01

## associate-file@1

include: DocsRepositoryActionsTests.DocsRepositoryActionsSettings_RepositoryActionAssociateFileV1.verified.md

Example:

snippet: RepositoryActionsAssociateFile01

## browse-repository@1

include: DocsRepositoryActionsTests.DocsRepositoryActionsSettings_RepositoryActionBrowseRepositoryV1.verified.md

Example:

snippet: RepositoryActionsBrowseRepository01

## browser@1

include: DocsRepositoryActionsTests.DocsRepositoryActionsSettings_RepositoryActionBrowserV1.verified.md

Example:

snippet: RepositoryActionsBrowser01

## command@1

include: DocsRepositoryActionsTests.DocsRepositoryActionsSettings_RepositoryActionCommandV1.verified.md

Example:

snippet: RepositoryActionsCommand01

## executable@1

include: DocsRepositoryActionsTests.DocsRepositoryActionsSettings_RepositoryActionExecutableV1.verified.md

When you want to specify exacly one executable, you can replace the required property `Executables` with the following property:

- `executable`: Absolute path of the exeuctable to execute (required, string, evaluted)

Example:

snippet: RepositoryActionsExecutable01

## folder@1

include: DocsRepositoryActionsTests.DocsRepositoryActionsSettings_RepositoryActionFolderV1.verified.md

Example:

snippet: RepositoryActionsFolder01

## foreach@1

include: DocsRepositoryActionsTests.DocsRepositoryActionsSettings_RepositoryActionForeachV1.verified.md

Example:

snippet: RepositoryActionsForeach01

## git-checkout@1

include: DocsRepositoryActionsTests.DocsRepositoryActionsSettings_RepositoryActionGitCheckoutV1.verified.md

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

include: _plugins.azuredevops.action

See the [AzureDevOps](RepoM.Plugin.AzureDevOps.md) plugin for more information.

include: _plugins.heidi.action

See the [Heidi](RepoM.Plugin.Heidi.md) plugin for more information.

# Repository Actions

These actions are part of the Repository Actions config file described in [Repository Actions](RepositoryActions.md).
