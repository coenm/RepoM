# RepoM

[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=RepoM&metric=alert_status)](https://sonarcloud.io/summary/new_code?id=RepoM)
[![Reliability Rating](https://sonarcloud.io/api/project_badges/measure?project=RepoM&metric=reliability_rating)](https://sonarcloud.io/summary/new_code?id=RepoM)
[![Coverage](https://sonarcloud.io/api/project_badges/measure?project=RepoM&metric=coverage)](https://sonarcloud.io/summary/new_code?id=RepoM)
[![Lines of Code](https://sonarcloud.io/api/project_badges/measure?project=RepoM&metric=ncloc)](https://sonarcloud.io/summary/new_code?id=RepoM)
[![Code Smells](https://sonarcloud.io/api/project_badges/measure?project=RepoM&metric=code_smells)](https://sonarcloud.io/summary/new_code?id=RepoM)
[![Maintainability Rating](https://sonarcloud.io/api/project_badges/measure?project=RepoM&metric=sqale_rating)](https://sonarcloud.io/summary/new_code?id=RepoM)
[![Bugs](https://sonarcloud.io/api/project_badges/measure?project=RepoM&metric=bugs)](https://sonarcloud.io/summary/new_code?id=RepoM)

RepoM is a minimal-conf git repository hub. It uses the git repositories on your machine to create an efficient navigation widget and makes sure you'll never lose track of your work along the way.

It's populating itself as you work with git. It does not get in the way and only requires minimal effort to configure.

RepoM will not compete with your favourite git clients, so keep them. It's not about working within a repository: It's a new way to use all of your repositories to make your daily work easier.

If you recognise this introduction you might be right. RepoM is a fork of the popular [RepoZ](#credits).

## The Hub

The hub provides a quick overview of your repositories including their current branch, a short status information, and optionally some provided tags. Additionally, it offers some shortcuts like revealing a repository in the Windows Explorer, opening a command line tool in a given repository, checking out git branches and lots of other predefined or customizable actions.

![Screenshot](https://raw.githubusercontent.com/awaescher/RepoZ/master/_doc/RepoZ-ReadMe-UI-Both.png)

If you are working on different git repositories throughout the day, you might find yourself wasting time by permanently switching over from one repository to another. If you are like me, you tend to keep all those windows open to be reused later, ending up on a window list which has to be looped through all the time.

With RepoM, you can instantly jump into a given repository with a file browser or command prompt. This is shown in the following gif.

![Navigation](https://raw.githubusercontent.com/awaescher/RepoZ/master/_doc/QuickNavigation.gif)

For Windows, use the hotkeys <kbd>Ctrl</kbd>+<kbd>Alt</kbd>+<kbd>R</kbd> to show RepoM.

<!--
To open a file browser, simply press <kbd>Return</kbd> on the keyboard once you selected a repository. To open a command prompt instead, hold <kbd>Ctrl</kbd> on Windows while pressing <kbd>Return</kbd>. These modifier keys will also work with mouse navigation.
-->

## Configuration

## Context Menu

The main functionality of RepoM are the quick actions to execute per repository. For instance, you can quickly naviate to the repository by directly opening the windows explorer or by opening a command prompt. This context menu is user and repostirory specific and can be defined using yaml. This way, you can add an context menu item (action) for opening the repository in Visual Studio (for a C# project) and for an other repository you can add the action to open a repository in Eclipse.

To read more about the context menu, click here.

These actions are defined in the `RepositoryActionsV2.yaml` located in your `%APPDATA%\RepoM\` folder. More information can be found in the docs folder.

## Tagging

It is possible to dynamically assign tags to repositories such that you can filter, order, and search repositories using these tags.
How to define and use tags is described in the [Tags](docs/Tags.md) documentation.

## Search

It is possible to filter or search for repositories using the search box at the top of RepoM.
See the [Search](docs/search.md) for more information.

## Ordering and Filtering

The order of the repositories shown in RepoM is customizable. The default is alphabetical on the repository name. Read more about ordering [here](docs/_old/Ordering.md).

The repositories shown in RepoM are filtered using the search box in RepoM. But the default set can also be configured using different presets.

## Global configuration

When RepoM starts for the first time, a configuration file wil be created. Most of the properties can be adjusted using the UI but, at this moment, one property must be altered manually. Read more over [here](docs/_old/Settings.md).

## Plugins

RepoM uses plugins to extend functionality. At this moment, when a plugin is available in the installed directory, it will be found and can be enabled or disabled. This is done in the hamburger menu of RepoM. Enabling or disabling requires a restart of RepoM.

- [Plugins](docs/Plugins.md)
  - [AzureDevOps](docs/plugin_repom.plugin.azuredevops.generated.md)
  - [Clipboard](docs/plugin_repom.plugin.clipboard.generated.md)
  - [Heidi](docs/plugin_repom.plugin.heidi.generated.md)
  - [LuceneQueryParser](docs/plugin_repom.plugin.lucenequeryparser.generated.md)
  - [SonarCloud](docs/plugin_repom.plugin.sonarcloud.generated.md)
  - [Statistics](docs/plugin_repom.plugin.statistics.generated.md)
  - [WebBrowser](docs/plugin_repom.plugin.webbrowser.generated.md)
  - [WindowsExplorerGitInfo](docs/plugin_repom.plugin.windowsexplorergitinfo.generated.md)
  
## Credits

RepoM is a fork of [RepoZ](https://github.com/awaescher/RepoZ), which was created by [Andreas Wäscher](https://github.com/awaescher).
RepoZ contains functionality that has been stripped in RepoM like supporting MacOS, releasing versions using chocolatey, the commandline sidekick (`grr``), and performing actions at multiple repostitories at once.
