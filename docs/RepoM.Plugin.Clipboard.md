# Clipboard

When this module is enabled, you can create repository actions to copy specific (evaluated) text to the clipboard using the action provider type `clipboard-copy`.

## sonarcloud-set-favorite@1 <!-- include: _plugins.clipboard.action. path: /docs/mdsource/_plugins.clipboard.action.include.md -->

Action to mark a repository as favorite within SonarCloud. This action requires the use of the SonarCloud plugin.

Custom properties:

- Name: The name of the item (required, string, evaulated)
- Enabled: If the action is clickable (optional, boolean/string, evaluated, default true)
- Project: The SonarCloud project key (required, string, evaluated)

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
