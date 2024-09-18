# Actions

Most of the repository actions are part of the core of RepoM and some are part of external plugins. All these repository actions have the same base:

Properties:<!-- include: DocsRepositoryActionsTests.RepositoryActionBaseDocumentationGeneration_RepositoryAction.verified.md -->

- `type`: RepositoryAction type. Should be a fixed value used to determine the action type. (required, string)
- `name`: Name of the action. This is shown in the UI of RepoM. (required, evaluated, string)
- `active`: Is the action active (ie. visible) or not. (optional, evaluated, boolean, default: `true`)
- `variables`: A set of variables to be availabe within this action. (optional, list`1)<!-- endInclude -->

The following actions are part of the core of RepoM and can always be used in your RepositoryActions.

## browse-repository@1

Action to open the default webbrowser and go to the origin remote webinterface. When multple remotes are available a sub menu is created for each remote.<!-- include: DocsRepositoryActionsTests.DocsRepositoryActionsSettings_RepositoryActionBrowseRepositoryV1.verified.md -->

Action specific properties:

- `first-only`: Property specifying only a menu item for the first remote is created. (optional, evaluated, boolean)<!-- endInclude -->

Example:

<!-- snippet: RepositoryActionsBrowseRepository01 -->
<a id='snippet-repositoryactionsbrowserepository01'></a>
```yaml
repository-actions:
  actions:
  - type: browse-repository@1
    active: true 
    variables: [] # default property but doens't add anything to this action

  - type: browse-repository@1
```
<sup><a href='/tests/RepoM.Api.Tests/IO/ModuleBasedRepositoryActionProvider/DocumentationFiles/BrowseRepository01.testfile.yaml#L3-L13' title='Snippet source file'>snippet source</a> | <a href='#snippet-repositoryactionsbrowserepository01' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

## executable@1

Action to excute an application with additional arguments. This action is almost identical to the `command@1` action. When no existing executables are provided, the action will not show.<!-- include: DocsRepositoryActionsTests.DocsRepositoryActionsSettings_RepositoryActionExecutableV1.verified.md -->

Action specific properties:

- `executables`: Set of possible executables. The first executable that exists will be used. The paths should absolute. (required, evaluated, string)
- `arguments`: Arguments for the executable. (optional, evaluated, string)<!-- endInclude -->

When you want to specify exacly one executable, you can replace the required property `Executables` with the following property:

- `executable`: Absolute path of the exeuctable to execute (required, string, evaluted)

Example:

<!-- snippet: RepositoryActionsExecutable01 -->
<a id='snippet-repositoryactionsexecutable01'></a>
```yaml
repository-actions:
  actions:

  # all properties
  - type: executable@1
    active: true 
    variables: []
    name: 'Open in Visual Studio Code'
    executables:
    - '%LocalAppData%/Programs/Microsoft VS Code/code.exe'
    - '%ProgramW6432%/Microsoft VS Code/code.exe'
    arguments: '"{Repository.SafePath}"'

  # replace executables array with property executable
  - type: executable@1
    active: true 
    variables: []
    name: 'Open in Visual Studio Code'
    executable: '%LocalAppData%/Programs/Microsoft VS Code/code.exe'
    arguments: '"{Repository.SafePath}"'
    
  # without default values for active and variables.
  - type: executable@1
    name: 'Open in Visual Studio Code'
    executables:
    - '%LocalAppData%/Programs/Microsoft VS Code/code.exe'
    - '%ProgramW6432%/Microsoft VS Code/code.exe'
    arguments: '"{Repository.SafePath}"'

  - type: executable@1
    name: 'Open in Visual Studio Code'
    executable: '%LocalAppData%/Programs/Microsoft VS Code/code.exe'
    arguments: '"{Repository.SafePath}"'
```
<sup><a href='/tests/RepoM.Api.Tests/IO/ModuleBasedRepositoryActionProvider/DocumentationFiles/Executable01.testfile.yaml#L3-L39' title='Snippet source file'>snippet source</a> | <a href='#snippet-repositoryactionsexecutable01' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

## folder@1

Action to create a folder (sub menu) in the context menu of the repository allowing you to order actions.<!-- include: DocsRepositoryActionsTests.DocsRepositoryActionsSettings_RepositoryActionFolderV1.verified.md -->

Action specific properties:

- `items`: Menu items. (required, evaluated, list`1)
- `is-deferred`: Menu is deferred. This will speed up visualisation. (optional, evaluated, boolean, default: `false`)<!-- endInclude -->

Example:

<!-- snippet: RepositoryActionsFolder01 -->
<a id='snippet-repositoryactionsfolder01'></a>
```yaml
repository-actions:
  actions:
  - type: folder@1
    active: true 
    variables: []
    name: 'My Folder 1'
    items:
    - type: just-text@1
      name: 'text 1 in sub menu'
    - type: just-text@1
      name: 'text 2 in sub menu'

  - type: folder@1
    name: 'My Folder 2'
    items:
    - type: just-text@1
      name: 'text 1 in sub menu'
    - type: just-text@1
      name: 'text 2 in sub menu'
```
<sup><a href='/tests/RepoM.Api.Tests/IO/ModuleBasedRepositoryActionProvider/DocumentationFiles/Folder01.testfile.yaml#L3-L25' title='Snippet source file'>snippet source</a> | <a href='#snippet-repositoryactionsfolder01' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

## git-checkout@1

This action will create a menu and sub menus with all local and remote branches for an easy checkout.<!-- include: DocsRepositoryActionsTests.DocsRepositoryActionsSettings_RepositoryActionGitCheckoutV1.verified.md -->

This action does not have any specific properties.<!-- endInclude -->

Example:

<!-- snippet: RepositoryActionsGitCheckout01 -->
<a id='snippet-repositoryactionsgitcheckout01'></a>
```yaml
repository-actions:
  actions:
  - type: git-checkout@1
    active: true 
    variables: [] # default property but doens't add anything to this action

  - type: git-checkout@1
```
<sup><a href='/tests/RepoM.Api.Tests/IO/ModuleBasedRepositoryActionProvider/DocumentationFiles/GitCheckout01.testfile.yaml#L3-L13' title='Snippet source file'>snippet source</a> | <a href='#snippet-repositoryactionsgitcheckout01' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

## git-fetch@1

Action to execute a `git fetch` command.<!-- include: DocsRepositoryActionsTests.DocsRepositoryActionsSettings_RepositoryActionGitFetchV1.verified.md -->

This action does not have any specific properties.<!-- endInclude -->

Example:

<!-- snippet: RepositoryActionsGitFetch01 -->
<a id='snippet-repositoryactionsgitfetch01'></a>
```yaml
repository-actions:
  actions:
  - type: git-fetch@1
    active: true 
    variables: [] # default property but doens't add anything to this action

  - type: git-fetch@1
```
<sup><a href='/tests/RepoM.Api.Tests/IO/ModuleBasedRepositoryActionProvider/DocumentationFiles/GitFetch01.testfile.yaml#L3-L13' title='Snippet source file'>snippet source</a> | <a href='#snippet-repositoryactionsgitfetch01' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

## git-pull@1

Action to execute a `git pull` command.<!-- include: DocsRepositoryActionsTests.DocsRepositoryActionsSettings_RepositoryActionGitPullV1.verified.md -->

This action does not have any specific properties.<!-- endInclude -->

Example:

<!-- snippet: RepositoryActionsGitPull01 -->
<a id='snippet-repositoryactionsgitpull01'></a>
```yaml
repository-actions:
  actions:
  - type: git-pull@1
    active: true 
    variables: [] # default property but doens't add anything to this action

  - type: git-pull@1
```
<sup><a href='/tests/RepoM.Api.Tests/IO/ModuleBasedRepositoryActionProvider/DocumentationFiles/GitPull01.testfile.yaml#L3-L13' title='Snippet source file'>snippet source</a> | <a href='#snippet-repositoryactionsgitpull01' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

## git-push@1

Action to execute a `git push` command.<!-- include: DocsRepositoryActionsTests.DocsRepositoryActionsSettings_RepositoryActionGitPushV1.verified.md -->

This action does not have any specific properties.<!-- endInclude -->

Example:

<!-- snippet: RepositoryActionsGitPush01 -->
<a id='snippet-repositoryactionsgitpush01'></a>
```yaml
repository-actions:
  actions:
  - type: git-push@1
    active: true 
    variables: [] # default property but doens't add anything to this action

  - type: git-push@1
```
<sup><a href='/tests/RepoM.Api.Tests/IO/ModuleBasedRepositoryActionProvider/DocumentationFiles/GitPush01.testfile.yaml#L3-L13' title='Snippet source file'>snippet source</a> | <a href='#snippet-repositoryactionsgitpush01' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

## ignore-repository@1

Action to ignore the current repository. This repository will be added to the list of ignored repositories and will never show in RepoM.<!-- include: DocsRepositoryActionsTests.DocsRepositoryActionsSettings_RepositoryActionIgnoreRepositoryV1.verified.md -->
To undo this action, clear all ignored repositories or manually edit the ignored repositories file (when RepoM is not running).

This action does not have any specific properties.<!-- endInclude -->

Example:

<!-- snippet: RepositoryActionsIgnoreRepository01 -->
<a id='snippet-repositoryactionsignorerepository01'></a>
```yaml
repository-actions:
  actions:
  - type: ignore-repository@1
    active: true 
    variables: [] # default property but doens't add anything to this action

  - type: ignore-repository@1
```
<sup><a href='/tests/RepoM.Api.Tests/IO/ModuleBasedRepositoryActionProvider/DocumentationFiles/IgnoreRepository01.testfile.yaml#L3-L13' title='Snippet source file'>snippet source</a> | <a href='#snippet-repositoryactionsignorerepository01' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

## pin-repository@1

Action to pin (or unpin) the current repository. Pinning is not persistant and all pinned repositories will be cleared when RepoM exits.<!-- include: DocsRepositoryActionsTests.DocsRepositoryActionsSettings_RepositoryActionPinRepositoryV1.verified.md -->
Pinning a repository allowed custom filtering, ordering and searching.

Action specific properties:

- `name`: Name of the action. This is shown in the UI of RepoM. When no value is provided, the name will be a default value based on the mode. (optional, evaluated, string)
- `mode`: The pin mode `[Toggle, Pin, UnPin]`. (required, pinmode)<!-- endInclude -->

Example:

<!-- snippet: RepositoryActionsPinRepository01 -->
<a id='snippet-repositoryactionspinrepository01'></a>
```yaml
repository-actions:
  actions:
  - type: pin-repository@1
    active: true 
    variables: []
    name: this is some text to display
    mode: toggle

  - type: pin-repository@1
    mode: toggle

  - type: pin-repository@1
    mode: pin

  - type: pin-repository@1
    mode: unpin
```
<sup><a href='/tests/RepoM.Api.Tests/IO/ModuleBasedRepositoryActionProvider/DocumentationFiles/PinRepository01.testfile.yaml#L3-L22' title='Snippet source file'>snippet source</a> | <a href='#snippet-repositoryactionspinrepository01' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

# Repository Actions

These actions are part of the Repository Actions config file described in [Repository Actions](RepositoryActions.md).
