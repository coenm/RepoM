# Repository Actions

Repository actions are described in a `yaml` file. Currently version 1 has the following sections.

<!-- snippet: RepositoryActionsV1Yaml -->
<a id='snippet-repositoryactionsv1yaml'></a>
```yaml
# file content version.
version: 1

# 'repository-specific-env-files' is an array and specifies loading specific environment files
# loaded environment variables can be used in actions.
repository-specific-env-files: []

# 'variables' is an array.
# variables can be refrerenced in actions.
variables: []

# Tags can be assigned to repositories based on predicates.
repository-tags:
  variables: []
  tags: []

# Repository actions which correspondent with the context menu of RepoM.
repository-actions:
  variables: []
  actions: []
```
<sup><a href='/tests/RepoM.Api.Tests/IO/ModuleBasedRepositoryActionProvider/TestFiles/Version1.testfile.yaml#L1-L24' title='Snippet source file'>snippet source</a> | <a href='#snippet-repositoryactionsv1yaml' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

## Repository Specific Env Files

In this section you can load custom environment (`.env`) files. These environment variables can be used within actions.
This configuration will be evaluated and loaded every time the context menu of a repository opens. Using the `when` keyword, it is possible to only load environment files for specific repositories.

<!-- snippet: RepositoryActionsV1RepositorySpecificEnvFilesYaml -->
<a id='snippet-repositoryactionsv1repositoryspecificenvfilesyaml'></a>
```yaml
# array of objects.
repository-specific-env-files:

  # 'filename' is the filename to load environment variables from (required, string, evaluated).
- filename: ''

  # 'when' is a predicate when to load the file (optional, string/boolean, evaluated).
  when: ''

- filename: ''
  when: ''
- filename: ''
  when: ''
```
<sup><a href='/tests/RepoM.Api.Tests/IO/ModuleBasedRepositoryActionProvider/TestFiles/RepositorySpecificEnvFilesExplanation1.testfile.yaml#L1-L17' title='Snippet source file'>snippet source</a> | <a href='#snippet-repositoryactionsv1repositoryspecificenvfilesyaml' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

## Variables

Defined variables can be referenced within actions using the variable prefix `var.`. Other variable providers are available within RepoM and are explained over here.
It is possible to define variables at multiple levels (ie scopes). This is the top level of defining variables.

<!-- snippet: RepositoryActionsV1RepositoryVariablesYaml -->
```
** Could not find snippet 'RepositoryActionsV1RepositoryVariablesYaml' **
```
<!-- endSnippet -->

## Repository Tags

This section can be used to assign tags to repositories. These tags can be used to filter, order or search repositories.

<!-- snippet: RepositoryActionsV1RepositoryTagsYaml -->
<a id='snippet-repositoryactionsv1repositorytagsyaml'></a>
```yaml
# array of objects.
repository-tags:
  # variable section for tags and has the same structure as the global variable section (optional, default empty)
  variables:

  tags:

    # 'tag' is the name of the tag (required, string, no-spaces).
  - tag: ''
  
    # 'when' is a predicate when to assign the tag to a repository (optional, string/boolean, evaluated, default true).
    when: ''

  - tag: ''
    when: ''
  - tag: ''
    when: ''
```
<sup><a href='/tests/RepoM.Api.Tests/IO/ModuleBasedRepositoryActionProvider/TestFiles/RepositoryTagsExplanation1.testfile.yaml#L1-L21' title='Snippet source file'>snippet source</a> | <a href='#snippet-repositoryactionsv1repositorytagsyaml' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

## Repository Actions

This section describes the whole context menu of the repository. Within the `repository-actions` it is possible to define variables at multiple levels. The `actions` subsection contains all the actions.
Actions can be enabled or disabled based on all kind of conditions. RepoM comes with a number of actions but it is also possible to load plugins containg specific actions.

<!-- snippet: RepositoryActionsV1RepositoryActionsYaml -->
```
** Could not find snippet 'RepositoryActionsV1RepositoryActionsYaml' **
```
<!-- endSnippet -->
