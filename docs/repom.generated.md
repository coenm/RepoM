# RepoM Core Repository Actions

The following actions are part of the core of RepoM and can always be used in your RepositoryActions.

## browse-repository@1

Action to open the default webbrowser and go to the origin remote webinterface. When multiple remotes are available a sub menu is created for each remote.

Properties:

- `name`: Name of the menu item. ([Text](repository_action_types.md#text))
- `active`: Whether the menu item is enabled. ([Predicate](repository_action_types.md#predicate))
- `first-only`: Single menu for the first remote. ([Predicate](repository_action_types.md#predicate))

## command@1

Action to excute a command (related to the repository)

Properties:

- `name`: Name of the menu item. ([Text](repository_action_types.md#text))
- `active`: Whether the menu item is enabled. ([Predicate](repository_action_types.md#predicate))
- `command`: The command to execute. ([Text](repository_action_types.md#text))
- `arguments`: Arguments for the command. ([Text](repository_action_types.md#text))

### Example

<!-- snippet: command@1-scenario01 -->
<a id='snippet-command@1-scenario01'></a>
```yaml
action-menu:

- type: command@1
  name: Open in Windows Terminal
  command: wt
  arguments: -d "{{ repository.linux_path }}"

- type: command@1
  name: Commit and Push
  command: cmd
  arguments: /k cd "{{ repository.path }}" & git add . & git commit -m "my fix" & git push & exit
```
<sup><a href='/tests/RepoM.ActionMenu.Core.Tests/ActionMenu/IntegrationTests/CommandV1Tests.DocumentationScenario01.testfile.yaml#L3-L17' title='Snippet source file'>snippet source</a> | <a href='#snippet-command@1-scenario01' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->


## executable@1

Action to excute an application with additional arguments. This action is almost identical to the `command@1` action. When no existing executables are provided, the action will not show.

Properties:

- `name`: Name of the menu item. ([Text](repository_action_types.md#text))
- `active`: Whether the menu item is enabled. ([Predicate](repository_action_types.md#predicate))
- `executable`: The executable. ([Text](repository_action_types.md#text))
- `arguments`: Arguments for the executable. ([Text](repository_action_types.md#text))
- `context`: The context in which the action is available. ([Context](repository_action_types.md#context))

## folder@1

Action to create a folder (sub menu) in the context menu of the repository allowing you to order actions.

Properties:

- `actions`: List of actions. (ActionMenu, optional)
- `name`: Name of the menu item. ([Text](repository_action_types.md#text))
- `active`: Whether the menu item is enabled. ([Predicate](repository_action_types.md#predicate))
- `context`: The context in which the action is available. ([Context](repository_action_types.md#context))

### Example

<!-- snippet: folder@1-scenario01 -->
```
** Could not find snippet 'folder@1-scenario01' **
```
<!-- endSnippet -->


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

<!-- snippet: foreach@1-scenario01 -->
<a id='snippet-foreach@1-scenario01'></a>
```yaml
context:
- type: evaluate-script@1
  content: |-
    func sanitize_filename_testproject(path)
      ret path | string.split("\\") | array.last | string.replace(".Tests.csproj", "")
    end

    test_projects = file.find_files(repository.windows_path, "*.Tests.csproj");

action-menu:

- type: foreach@1
  enumerable: test_projects
  variable: test_project
  actions:
  - type: command@1
    name: execute dotnet test '{{ sanitize_filename_testproject(test_project) }}'
    command: cmd
    arguments: /k dotnet test -c release "{{ test_project }}" --verbosity q
```
<sup><a href='/tests/RepoM.ActionMenu.Core.Tests/ActionMenu/IntegrationTests/ForEachV1Tests.Documentation.foreach@1-scenario01.testfile.yaml#L1-L23' title='Snippet source file'>snippet source</a> | <a href='#snippet-foreach@1-scenario01' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

<!-- snippet: foreach@1-scenario02 -->
<a id='snippet-foreach@1-scenario02'></a>
```yaml
action-menu:

- type: foreach@1
  enumerable: repository.remotes
  variable: remote
  actions:
  - type: url@1
    name: 'Browse to remote {{ remote.key}}'
    url: '{{ remote.url }}'
    active: remote.url | string.starts_with 'https'
```
<sup><a href='/tests/RepoM.ActionMenu.Core.Tests/ActionMenu/IntegrationTests/ForEachV1Tests.Documentation.foreach@1-scenario02.testfile.yaml#L3-L16' title='Snippet source file'>snippet source</a> | <a href='#snippet-foreach@1-scenario02' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->


## git-checkout@1

This action will create a menu and sub menus with all local and remote branches for an easy checkout.

Properties:

- `name`: Name of the menu item. ([Text](repository_action_types.md#text))
- `active`: Whether the menu item is enabled. ([Predicate](repository_action_types.md#predicate))

### Example

<!-- snippet: git-checkout@1-scenario01 -->
<a id='snippet-git-checkout@1-scenario01'></a>
```yaml
action-menu:
- type: git-checkout@1

- type: git-checkout@1
  name: Checkout!
  active: true
```
<sup><a href='/tests/RepoM.ActionMenu.Core.Tests/ActionMenu/IntegrationTests/GitCheckoutV1Tests.Documentation.git-checkout@1-scenario01.testfile.yaml#L3-L12' title='Snippet source file'>snippet source</a> | <a href='#snippet-git-checkout@1-scenario01' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->


## git-fetch@1

Action to execute a `git fetch` command.

Properties:

- `name`: Name of the menu item. ([Text](repository_action_types.md#text))
- `active`: Whether the menu item is enabled. ([Predicate](repository_action_types.md#predicate))

### Example

<!-- snippet: git-fetch@1-scenario01 -->
<a id='snippet-git-fetch@1-scenario01'></a>
```yaml
action-menu:
- type: git-fetch@1

- type: git-fetch@1
  name: Fetch!
  active: true
```
<sup><a href='/tests/RepoM.ActionMenu.Core.Tests/ActionMenu/IntegrationTests/GitFetchV1Tests.Documentation.git-fetch@1-scenario01.testfile.yaml#L3-L12' title='Snippet source file'>snippet source</a> | <a href='#snippet-git-fetch@1-scenario01' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->


## git-pull@1

Action to execute a `git pull` command.

Properties:

- `name`: Name of the menu item. ([Text](repository_action_types.md#text))
- `active`: Whether the menu item is enabled. ([Predicate](repository_action_types.md#predicate))

### Example

<!-- snippet: git-pull@1-scenario01 -->
<a id='snippet-git-pull@1-scenario01'></a>
```yaml
action-menu:
- type: git-pull@1

- type: git-pull@1
  name: Pull!
  active: true
```
<sup><a href='/tests/RepoM.ActionMenu.Core.Tests/ActionMenu/IntegrationTests/GitPullV1Tests.Documentation.git-pull@1-scenario01.testfile.yaml#L3-L12' title='Snippet source file'>snippet source</a> | <a href='#snippet-git-pull@1-scenario01' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->


## git-push@1

Action to execute a `git push` command.

Properties:

- `name`: Name of the menu item. ([Text](repository_action_types.md#text))
- `active`: Whether the menu item is enabled. ([Predicate](repository_action_types.md#predicate))

### Example

<!-- snippet: git-push@1-scenario01 -->
<a id='snippet-git-push@1-scenario01'></a>
```yaml
action-menu:
- type: git-push@1

- type: git-push@1
  name: Push!
  active: true
```
<sup><a href='/tests/RepoM.ActionMenu.Core.Tests/ActionMenu/IntegrationTests/GitPushV1Tests.Documentation.git-push@1-scenario01.testfile.yaml#L3-L12' title='Snippet source file'>snippet source</a> | <a href='#snippet-git-push@1-scenario01' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->


## ignore-repository@1

Action to ignore the current repository. This repository will be added to the list of ignored repositories and will never show in RepoM.
To undo this action, clear all ignored repositories or manually edit the ignored repositories file (when RepoM is not running).

Properties:

- `name`: Name of the menu item. ([Text](repository_action_types.md#text))
- `active`: Whether the menu item is enabled. ([Predicate](repository_action_types.md#predicate))
- `context`: The context in which the action is available. ([Context](repository_action_types.md#context))

## just-text@1

Textual action to display some text in the action menu.

Properties:

- `name`: Name of the menu item. ([Text](repository_action_types.md#text))
- `active`: Whether the menu item is enabled. ([Predicate](repository_action_types.md#predicate))
- `enabled`: Show the menu as enabled (clickable) or disabled. ([Predicate](repository_action_types.md#predicate))
- `context`: The context in which the action is available. ([Context](repository_action_types.md#context))

### Example

<!-- snippet: just-text@1-scenario01 -->
<a id='snippet-just-text@1-scenario01'></a>
```yaml
context:

- type: evaluate-script@1
  content: |-
    now = date.parse '20/01/2022 08:32:48 +00:00' culture:'en-GB'

action-menu:

- type: just-text@1
  name: Static text with conditional active
  active: '1 < 10 && now.year == 2022' # true

- type: just-text@1
  name: Dynamic text {{ now | date.to_string "%Y-%m-%d"}}

- type: just-text@1
  name: Dynamic text with additional context {{ my_app_name }} - Year {{ now_year }}
  context:
  - type: evaluate-variable@1
    name: now_year
    value: now | date.to_string "%Y"
  - type: set-variable@1
    name: my_app_name
    value: RepoM
```
<sup><a href='/tests/RepoM.ActionMenu.Core.Tests/ActionMenu/IntegrationTests/JustTextV1Tests.DocumentationScenario01.testfile.yaml#L1-L28' title='Snippet source file'>snippet source</a> | <a href='#snippet-just-text@1-scenario01' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->


## pin-repository@1

Action to pin (or unpin) the current repository. Pinning is not persistant and all pinned repositories will be cleared when RepoM exits.
Pinning a repository allowed custom filtering, ordering and searching.

Properties:

- `name`: Name of the menu item. ([Text](repository_action_types.md#text))
- `active`: Whether the menu item is enabled. ([Predicate](repository_action_types.md#predicate))
- `context`: The context in which the action is available. ([Context](repository_action_types.md#context))
- `mode`: The pin mode `[Toggle, Pin, UnPin]`. (Nullable, optional)

## separator@1

Creates a visual separator in the action menu.

Properties:

- `active`: Whether the menu item is enabled. ([Predicate](repository_action_types.md#predicate))
- `context`: The context in which the action is available. ([Context](repository_action_types.md#context))

### Example

<!-- snippet: separator@1-scenario01 -->
<a id='snippet-separator@1-scenario01'></a>
```yaml
action-menu:

- type: separator@1
  active: 'string.size(my_app_name) == 5' # true
  context:
  - type: set-variable@1
    name: my_app_name
    value: RepoM

- type: separator@1
```
<sup><a href='/tests/RepoM.ActionMenu.Core.Tests/ActionMenu/IntegrationTests/SeparatorV1Tests.DocumentationScenario01.testfile.yaml#L3-L16' title='Snippet source file'>snippet source</a> | <a href='#snippet-separator@1-scenario01' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->


## url@1

Action to open the url in the default browser.

Properties:

- `name`: Name of the menu item. ([Text](repository_action_types.md#text))
- `url`: The URL to browse to. ([Text](repository_action_types.md#text))
- `active`: Whether the menu item is enabled. ([Predicate](repository_action_types.md#predicate))
- `context`: The context in which the action is available. ([Context](repository_action_types.md#context))

### Example

<!-- snippet: url@1-scenario01 -->
<a id='snippet-url@1-scenario01'></a>
```yaml
action-menu:

- type: folder@1
  name: Urls
  active: true
  actions:
  - type: url@1
    name: 'Browse to remote {{ repository.remotes[0].key }}'
    url: '{{ repository.remotes[0].url }}'
    active: repository.remotes[0].url | string.starts_with 'https'
  - type: url@1
    name: 'wiki'
    url: '{{ repository.remotes[0].url }}/wiki'
```
<sup><a href='/tests/RepoM.ActionMenu.Core.Tests/ActionMenu/IntegrationTests/FolderV1Tests.Documentation.folder@1-scenario01.testfile.yaml#L3-L19' title='Snippet source file'>snippet source</a> | <a href='#snippet-url@1-scenario01' title='Start of snippet'>anchor</a></sup>
<a id='snippet-url@1-scenario01-1'></a>
```yaml
context:

- type: evaluate-script@1
  content: |-
    now = date.parse '20/01/2022 08:32:48 +00:00' culture:'en-GB'

action-menu:

- type: url@1
  name: 'Wiki'
  url: 'https://github.com/coenm/RepoM/wiki'
  active: 'repository.path | string.contains RepoM'

- type: folder@1
  name: Link to all remotes of repo
  active: array.size(repository.remotes) >= 1
  actions:
  - type: foreach@1
    enumerable: repository.remotes
    variable: remote
    actions:
    - type: url@1
      name: 'Browse to remote {{ remote.key}}'
      url: '{{ remote.url }}'
      active: remote.url | string.starts_with 'https'
```
<sup><a href='/tests/RepoM.ActionMenu.Core.Tests/ActionMenu/IntegrationTests/UrlV1Tests.DocumentationScenario01.testfile.yaml#L1-L29' title='Snippet source file'>snippet source</a> | <a href='#snippet-url@1-scenario01-1' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

