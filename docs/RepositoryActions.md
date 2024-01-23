# Repository Actions

Repository actions are described in a `yaml` file. Currently version 1 has the following sections.

<!-- snippet: RepositoryActionsV1Yaml -->
```
** Could not find snippet 'RepositoryActionsV1Yaml' **
```
<!-- endSnippet -->

## Repository Specific Env Files

In this section you can load custom environment (`.env`) files. These environment variables can be used within actions.
This configuration will be evaluated and loaded every time the context menu of a repository opens. Using the `when` keyword, it is possible to only load environment files for specific repositories.

<!-- snippet: RepositoryActionsV1RepositorySpecificEnvFilesYaml -->
```
** Could not find snippet 'RepositoryActionsV1RepositorySpecificEnvFilesYaml' **
```
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
```
** Could not find snippet 'RepositoryActionsV1RepositoryTagsYaml' **
```
<!-- endSnippet -->

## Repository Actions

This section describes the whole context menu of the repository. Within the `repository-actions` it is possible to define variables at multiple levels. The `actions` subsection contains all the actions.
Actions can be enabled or disabled based on all kind of conditions. RepoM comes with a number of actions but it is also possible to load plugins containg specific actions.

<!-- snippet: RepositoryActions01Base -->
```
** Could not find snippet 'RepositoryActions01Base' **
```
<!-- endSnippet -->

A list of currently supported actions can be found [here](ActionList.md).
