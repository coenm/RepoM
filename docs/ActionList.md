# Actions

Most of the repository actions are part of the core of RepoM and some are part of external plugins. All these repository actions have the same base:

Properties:<!-- include: DocsRepositoryActionsTests.RepositoryActionBaseDocumentationGeneration_RepositoryAction.verified.md -->

- `type`: RepositoryAction type. Should be a fixed value used to determine the action type. (required, string)
- `name`: Name of the action. This is shown in the UI of RepoM. (required, evaluated, string)
- `active`: Is the action active (ie. visible) or not. (optional, evaluated, boolean, default: `true`)
- `variables`: A set of variables to be availabe within this action. (optional, list`1)<!-- endInclude -->

The following actions are part of the core of RepoM and can always be used in your RepositoryActions.

## just-text@1

Textual action to display some text in the action menu.<!-- include: DocsRepositoryActionsTests.DocsRepositoryActionsSettings_RepositoryActionJustTextV1.verified.md -->

Action specific properties:

- `enabled`: Show the menu as enabled (clickable) or disabled. (optional, evaluated, boolean, default: `true`)<!-- endInclude -->

Example:

<!-- snippet: RepositoryActionsJustText01 -->
<a id='snippet-repositoryactionsjusttext01'></a>
```yaml
repository-actions:
  actions:
  - type: just-text@1
    active: true 
    variables: []
    enabled: true
    name: this is some text to display

  - type: just-text@1
    name: 'also text'
```
<sup><a href='/tests/RepoM.Api.Tests/IO/ModuleBasedRepositoryActionProvider/DocumentationFiles/JustText01.testfile.yaml#L3-L16' title='Snippet source file'>snippet source</a> | <a href='#snippet-repositoryactionsjusttext01' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

## associate-file@1

Action menu for opening files with a given extension. If files within the repository are found matching the extension, a submenu will be created with all matched files.<!-- include: DocsRepositoryActionsTests.DocsRepositoryActionsSettings_RepositoryActionAssociateFileV1.verified.md -->

Action specific properties:

- `extension`: The file extension to look for. This parameter can contain a combination of valid literal path and wildcard (`*` and `?`) characters, but it doesnt support regular expressions.
For example `*.sln`. (required, string)<!-- endInclude -->

Example:

<!-- snippet: RepositoryActionsAssociateFile01 -->
<a id='snippet-repositoryactionsassociatefile01'></a>
```yaml
repository-actions:
  actions:
  - type: associate-file@1
    active: true 
    variables: []
    name: 'Open in Visual Studio'
    extension: '*.sln'
    
  - type: associate-file@1
    name: 'Open in Visual Studio'
    extension: '*.sln'
```
<sup><a href='/tests/RepoM.Api.Tests/IO/ModuleBasedRepositoryActionProvider/DocumentationFiles/AssociateFile01.testfile.yaml#L3-L17' title='Snippet source file'>snippet source</a> | <a href='#snippet-repositoryactionsassociatefile01' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

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

## browser@1

Action opening a webbrowser with the provided url.<!-- include: DocsRepositoryActionsTests.DocsRepositoryActionsSettings_RepositoryActionBrowserV1.verified.md -->

Action specific properties:

- `url`: The url to browse to. (required, evaluated, string)<!-- endInclude -->

Example:

<!-- snippet: RepositoryActionsBrowser01 -->
<a id='snippet-repositoryactionsbrowser01'></a>
```yaml
repository-actions:
  actions:
  - type: browser@1
    active: true 
    variables: []
    name: 'My Github'
    url: 'https://github.com/coenm'

  - type: browser@1
    name: 'My Github'
    url: 'https://github.com/coenm'
```
<sup><a href='/tests/RepoM.Api.Tests/IO/ModuleBasedRepositoryActionProvider/DocumentationFiles/Browser01.testfile.yaml#L3-L17' title='Snippet source file'>snippet source</a> | <a href='#snippet-repositoryactionsbrowser01' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

## command@1

Action to excute a command (related the the repository)<!-- include: DocsRepositoryActionsTests.DocsRepositoryActionsSettings_RepositoryActionCommandV1.verified.md -->

Action specific properties:

- `command`: The command to execute. (required, evaluated, string)
- `arguments`: Arguments for the command. (optional, evaluated, string)<!-- endInclude -->

Example:

<!-- snippet: RepositoryActionsCommand01 -->
<a id='snippet-repositoryactionscommand01'></a>
```yaml
repository-actions:
  actions:
  - type: command@1
    active: true 
    variables: []
    name: Open in Terminal
    command: wt
    arguments: -d "{Repository.SafePath}"
    
  - type: command@1
    name: Open in Terminal
    command: wt
    arguments: -d "{Repository.SafePath}"
```
<sup><a href='/tests/RepoM.Api.Tests/IO/ModuleBasedRepositoryActionProvider/DocumentationFiles/Command01.testfile.yaml#L3-L19' title='Snippet source file'>snippet source</a> | <a href='#snippet-repositoryactionscommand01' title='Start of snippet'>anchor</a></sup>
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

- `items`: Menu items. (required, evaluated, list`1)<!-- endInclude -->

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

## foreach@1

Action to create repeated actions based on a variable.<!-- include: DocsRepositoryActionsTests.DocsRepositoryActionsSettings_RepositoryActionForeachV1.verified.md -->

Action specific properties:

- `enumerable`: The list of items to enumerate on. (required, evaluated, string)
- `variable`: The name of the variable to access to current enumeration of the  items. For each iteration, the variable `{var.name}` has the value of the current iteration. (required, evaluated, string)
- `skip`: Predicate to skip the current item. (optional, evaluated, string)
- `actions`: List of repeated actions. (required, evaluated, ienumerable`1)<!-- endInclude -->

Example:

<!-- snippet: RepositoryActionsForeach01 -->
<a id='snippet-repositoryactionsforeach01'></a>
```yaml
repository-actions:
  variables:
  - name: DTAP
    value:
    - key: D
      url: https://develop.local
    - key: T
      url: https://test.local
    - key: A
      url: https://acceptance.local
    - key: p
      url: https://production.local
  actions:
  - type: foreach@1
    enumerable: '{var.DTAP}'
    variable: environment
    skip: ''
    actions:
    - type: browser@1
      name: '{var.environment.key}'
      url: '{var.environment.url}'
```
<sup><a href='/tests/RepoM.Api.Tests/IO/ModuleBasedRepositoryActionProvider/DocumentationFiles/Foreach01.testfile.yaml#L3-L27' title='Snippet source file'>snippet source</a> | <a href='#snippet-repositoryactionsforeach01' title='Start of snippet'>anchor</a></sup>
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

## separator@1

Creates a visual separator in the action menu.<!-- include: DocsRepositoryActionsTests.DocsRepositoryActionsSettings_RepositoryActionSeparatorV1.verified.md -->

This action does not have any specific properties.<!-- endInclude -->

Example:

<!-- snippet: RepositoryActionsSeparator01 -->
<a id='snippet-repositoryactionsseparator01'></a>
```yaml
repository-actions:
  actions:
  - type: separator@1
    active: true 
    variables: [] # default property but doens't add anything to this action

  - type: separator@1
```
<sup><a href='/tests/RepoM.Api.Tests/IO/ModuleBasedRepositoryActionProvider/DocumentationFiles/Separator01.testfile.yaml#L3-L13' title='Snippet source file'>snippet source</a> | <a href='#snippet-repositoryactionsseparator01' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

# Plugin actions

These actions are available though the use of plugins.

## clipboard-copy@1<!-- include: _plugins.clipboard.action. path: /docs/mdsource/_plugins.clipboard.action.include.md -->

This action makes it possible to copy text to the clipboard.<!-- include: DocsRepositoryActionsTests.DocsRepositoryActionsSettings_RepositoryActionClipboardCopyV1.verified.md -->

Action specific properties:

- `text`: The text to copy to the clipboard. (required, evaluated, string)<!-- endInclude -->

Example:

<!-- snippet: RepositoryActionsClipboardCopy01 -->
<a id='snippet-repositoryactionsclipboardcopy01'></a>
```yaml
repository-actions:
  actions:
  - type: clipboard-copy@1
    active: true
    variables: []
    name: Copy to clipboard
    text: ''

  - type: clipboard-copy@1
    name: Copy to clipboard
    text: ''
```
<sup><a href='/tests/RepoM.Plugin.Clipboard.Tests/DocumentationFiles/Clipboard01.testfile.yaml#L3-L17' title='Snippet source file'>snippet source</a> | <a href='#snippet-repositoryactionsclipboardcopy01' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->
<!-- endInclude -->

See the [Clipboard](RepoM.Plugin.Clipboard.md) plugin for more information.

## sonarcloud-set-favorite@1<!-- include: _plugins.sonarcloud.action. path: /docs/mdsource/_plugins.sonarcloud.action.include.md -->

Action to mark a repository as favorite within SonarCloud.<!-- include: DocsRepositoryActionsTests.DocsRepositoryActionsSettings_RepositoryActionSonarCloudSetFavoriteV1.verified.md -->

Action specific properties:

- `project`: The SonarCloud project key. (required, evaluated, string)<!-- endInclude -->

Example:

<!-- snippet: RepositoryActionsSonarCloudSetFavorite01 -->
<a id='snippet-repositoryactionssonarcloudsetfavorite01'></a>
```yaml
repository-actions:
  actions:
  - type: sonarcloud-set-favorite@1
    active: true 
    variables: []
    name: Star repository on SonarCloud
    project: ''
    
  - type: sonarcloud-set-favorite@1
    name: Star repository on SonarCloud
    project: ''
```
<sup><a href='/tests/RepoM.Plugin.SonarCloud.Tests/DocumentationFiles/SonarCloudSetFavorite01.testfile.yaml#L3-L16' title='Snippet source file'>snippet source</a> | <a href='#snippet-repositoryactionssonarcloudsetfavorite01' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->
<!-- endInclude -->

See the [SonarCloud](RepoM.Plugin.SonarCloud.md) plugin for more information.

## azure-devops-create-prs@1<!-- include: _plugins.azuredevops.action. path: /docs/mdsource/_plugins.azuredevops.action.include.md -->

Action menu item to create a pull request in Azure Devops.<!-- include: DocsRepositoryActionsTests.DocsRepositoryActionsSettings_RepositoryActionAzureDevOpsCreatePullRequestsV1.verified.md -->

Action specific properties:

- `project-id`: The azure devops project id. (required, evaluated, string)
- `title`: Menu item title. When not provided, a title will be generated.
This property will be used instead of the Name property. (optional, string)
- `pr-title`: Pull Request title. When not provided, the title will be defined based on the branch name.
Title will be the last part of the branchname split on `/`, so `feature/123-testBranch` will result in title `123-testBranch` (optional, string)
- `to-branch`: Name of the branch the pull request should be merged into. For instance `develop`, or `main`. (required, string)
- `reviewer-ids`: List of reviewer ids. The id should be a valid Azure DevOps user id (ie. GUID). (optional, list`1)
- `draft-pr`: Boolean specifying if th PR should be marked as draft. (required, boolean, default: `false`)
- `include-work-items`: Boolean specifying if workitems should be included in the PR. The workitems will be found by using the commit messages. (required, boolean, default: `true`)
- `open-in-browser`: Boolean specifying if the Pull request should be opened in the browser after creation. (required, boolean, default: `false`)
- `auto-complete`: Auto complete options. Please take a look at the same for more information (required, repositoryactionazuredevopscreatepullrequestsautocompleteoptionsv1)<!-- endInclude -->

Example:

<!-- snippet: RepositoryActionsAzureDevopsCreatePrs01 -->
<a id='snippet-repositoryactionsazuredevopscreateprs01'></a>
```yaml
repository-actions:
  actions:
  # Create PR
  - type: azure-devops-create-prs@1
    project-id: ''
    to-branch: develop
    reviewer-ids: 
    - "GUID"

  # Create PR with auto-complete enabled
  - type: azure-devops-create-prs@1
    project-id: ''
    to-branch: develop
    reviewer-ids:
    - "GUID"
    auto-complete:
      enabled: true
      merge-strategy: "Squash"

  # Create PR with all settings
  - type: azure-devops-create-prs@1
    project-id: ''
    title: 'Create PR'
    # When no pr-title provided it will be generated based on convention.
    # Title will be the last part of the branchname split on '/'. 
    # For example: feature/testBranch will result in a PR title of 'testBranch'.
    pr-title: 'PR title' 
    to-branch: develop
    reviewer-ids:
    - "GUID"
    draft-pr: true
    include-work-items: true
    open-in-browser: false
    auto-complete:
      enabled: true
      merge-strategy: "NoFastForward" # You can choose from: "NoFastForward", "Squash", "Rebase" and "RebaseMerge"
      deleteSource-branch: true
      transition-work-items: true
```
<sup><a href='/tests/RepoM.Plugin.AzureDevOps.Tests/DocumentationFiles/AzureDevopsCreatePrs.testfile.yaml#L3-L44' title='Snippet source file'>snippet source</a> | <a href='#snippet-repositoryactionsazuredevopscreateprs01' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

## azure-devops-get-prs@1

This action results in zero or more items in the contextmenu. For each open pullrequest for the given repository, it will show an action to go to the specific PullRequest in your favorite webbrowser.<!-- include: DocsRepositoryActionsTests.DocsRepositoryActionsSettings_RepositoryActionAzureDevOpsGetPullRequestsV1.verified.md -->

Action specific properties:

- `project-id`: The azure devops project id. (required, evaluated, string)
- `repository-id`: The repository Id. If not provided, the repository id is located using the remote url. (optional, evaluated, string)
- `show-when-empty`: When no pull requests are available, this property is used to determine if no or a message item is showed. (optional, evaluated, string, default: `true`)<!-- endInclude -->

Example:

<!-- snippet: RepositoryActionsAzureDevopsGetPrs01 -->
<a id='snippet-repositoryactionsazuredevopsgetprs01'></a>
```yaml
repository-actions:
  actions:
  - type: azure-devops-get-prs@1
    active: true
    variables: []
    show-when-empty: true
    repository-id: ''
    project-id: ''

  - type: azure-devops-get-prs@1
    repository-id: ''
```
<sup><a href='/tests/RepoM.Plugin.AzureDevOps.Tests/DocumentationFiles/AzureDevopsGetPrs.testfile.yaml#L3-L17' title='Snippet source file'>snippet source</a> | <a href='#snippet-repositoryactionsazuredevopsgetprs01' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->
<!-- endInclude -->

See the [AzureDevOps](RepoM.Plugin.AzureDevOps.md) plugin for more information.

## heidi-databases@1<!-- include: _plugins.heidi.action. path: /docs/mdsource/_plugins.heidi.action.include.md -->

<!-- todo, improve docs -->
Action to list heidi databases and show action menus for them.<!-- include: DocsRepositoryActionsTests.DocsRepositoryActionsSettings_RepositoryActionHeidiDatabasesV1.verified.md -->

Action specific properties:

- `key`: Repository key.
If not provided, the repository `Remote.Origin.Name` is used as selector. (optional, string)
- `executable`: The absolute path of the Heidi executable. If not provided, the default value from the plugin settings is used. (optional, evaluated, string)<!-- endInclude -->

Example:

<!-- snippet: RepositoryActionsHeidiDatabases01 -->
<a id='snippet-repositoryactionsheididatabases01'></a>
```yaml
repository-actions:
  actions:
  - type: heidi-databases@1
    active: true
    variables: []
    name: Databases
    executable: ''

  - type: heidi-databases@1
    name: Databases
    executable: ''

  - type: heidi-databases@1
    name: Databases
```
<sup><a href='/tests/RepoM.Plugin.Heidi.Tests/DocumentationFiles/HeidiDatabases.testfile.yaml#L3-L20' title='Snippet source file'>snippet source</a> | <a href='#snippet-repositoryactionsheididatabases01' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->
<!-- endInclude -->

See the [Heidi](RepoM.Plugin.Heidi.md) plugin for more information.

# Repository Actions

These actions are part of the Repository Actions config file described in [Repository Actions](RepositoryActions.md).
