# Repository Actions

todo.

<!-- snippet: RepositoryActionsV1Yaml -->
<a id='snippet-repositoryactionsv1yaml'></a>
```yaml
# file content version.
version: 1

# 'repository-specific-env-files' is an array and specifies loading specific environment files
# loaded environment variables can be used in actions.
repository-specific-env-files: []

# 'repository-tags' is an array to specify tags for repositories.
# a tag is assigned to a repository when the 'when' predicate is true.
repository-tags: []

# 'variables' is an array.
# variables can be refrerenced in actions.
variables: []

# Repository actions which correspondent with the context menu of RepoM.
repository-actions:
  variables: []
  actions: []
```
<sup><a href='/tests/RepoM.Api.Tests/IO/ModuleBasedRepositoryActionProvider/TestFiles/Version1.testfile.yaml#L1-L23' title='Snippet source file'>snippet source</a> | <a href='#snippet-repositoryactionsv1yaml' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->
