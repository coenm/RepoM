# Actions

The following actions are part of the core of RepoM and can always be used in your RepositoryActions.

## just-text@1

Textual action to display some text in the action menu.

Custom properties:

- Name: The name of the item (required, string, evaulated)
- Enabled: If the action is clickable (optional, boolean/string, evaluated, default true)

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

*todo*

## browse-repository@1

Action to open the default webbrowser and go to the origin remote webinterface.

No additional properties and assigning variables has no effect.

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
<sup><a href='/tests/RepoM.Api.Tests/IO/ModuleBasedRepositoryActionProvider/DocumentationFiles/browse-repository01.testfile.yaml#L3-L13' title='Snippet source file'>snippet source</a> | <a href='#snippet-repositoryactionsbrowserepository01' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

## browser@1

Action opening a webbrowser with the provided url.

## command@1

*todo*

## executable@1

*todo*

## folder@1

Action to create a folder (sub menu) in the context menu of the repository allowing you to order actions.

## foreach@1

*todo*

## git-checkout@1

This action will create a menu and sub menus with all local and remote branches for an easy checkout.

No additional properties and assigning variables has no effect.

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

Action to execute a `git fetch` command.

No additional properties and assigning variables has no effect.

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

Action to execute a `git pull` command.

No additional properties and assigning variables has no effect.

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

Action to execute a `git push` command.

No additional properties and assigning variables has no effect.

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

## ignore-repositories@1

Action to ignore the current repository. This repository will be added to the list of ignored repositories and will never show in RepoM.
To undo this action, clear all ignored repositories or manually edit the ignored repositories file.

## pin-repository@1

Action to pin (or unpin) the current repository. Pinning is not persistant and all pinned repositories will be cleared when RepoM exits.
Pinning a repository allowed custom filtering, ordering and searching.

## separator@1

Creates a visual separator in the action menu.

No additional properties and assigning variables has no effect.

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
<sup><a href='/tests/RepoM.Api.Tests/IO/ModuleBasedRepositoryActionProvider/DocumentationFiles/Separator@1.testfile.yaml#L3-L13' title='Snippet source file'>snippet source</a> | <a href='#snippet-repositoryactionsseparator01' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

# Plugin actions

These actions are available though the use of plugins.

## clipboard-copy@1

This action makes it possible to copy text to the clipboard.

## sonarcloud-set-favorite@1

Action to mark a repository as favorite within SonarCloud. This action requires the use of the SonarCloud plugin.

## azure-devops-get-prs@1

This action results in zero or more items in the contextmenu. For each open pullrequest for the given repository, it will show an action to go to the specific PullRequest in your favorite webbrowser.
The AzureDevOps plugin is required.

## heidi-databases@1

*todo*

# Repository Actions

These actions are part of the Repository Actions config file described in [Repository Actions](RepositoryActions.md).
