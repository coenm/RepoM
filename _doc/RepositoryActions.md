# Repository Actions

todo.

<!-- snippet: RepositoryActionsV1Yaml -->
<a id='snippet-repositoryactionsv1yaml'></a>
```yaml
# Version 1 of the RepositoryActions.yaml, other versions are not supported at the moment.
version: 1

# 'repository-specific-env-files' is an array and specifies loading specific environment files
# to be used in actions.
repository-specific-env-files:
- filename: work.env
  when: '{StringContains({Repository.SafePath}, "work")}'
- filename: project-x.env

# 'repository-tags' is an array to specify tags for repositories.
# a tag is assigned to a repository when the 'when' predicate is true.
repository-tags:
- tag: work
  when: '{StringContains({Repository.SafePath}, "work")}'
- tag: always-tag

# 'variables' is an array to assign variables which can be used in actions. optional, defaults to empty.
variables:
- name: var1
  value: value1
  enabled: true
- name: varObject
  value:
    name: abc
    age: 12
    isSuperMan: true
    brothers:
    - BatMan
    - SuperMan
```
<sup><a href='/tests/RepoM.Api.Tests/IO/ModuleBasedRepositoryActionProvider/TestFiles/Version1.testfile.yaml#L1-L34' title='Snippet source file'>snippet source</a> | <a href='#snippet-repositoryactionsv1yaml' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->
