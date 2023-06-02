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

📦  [Check the Releases page](https://github.com/coenm/RepoM/releases) to **download** the latest version and see **what's new**!

## Credits

RepoM is a fork of [RepoZ](https://github.com/awaescher/RepoZ), which was created by [Andreas Wäscher](https://github.com/awaescher).
RepoZ contains functionality that has been stripped in RepoM like supporting MacOS, releasing versions using chocolatey, and the commandline sidekick (grr).

## The Hub

The hub provides a quick overview of your repositories including their current branch and a short status information. Additionally, it offers some shortcuts like revealing a repository in the Windows Explorer, opening a command line tool in a given repository and checking out git branches.

![Screenshot](https://raw.githubusercontent.com/awaescher/RepoZ/master/_doc/RepoZ-ReadMe-UI-Both.png)

If you are working on different git repositories throughout the day, you might find yourself wasting time by permanently switching over from one repository to another. If you are like me, you tend to keep all those windows open to be reused later, ending up on a window list which has to be looped through all the time.

With RepoM, you can instantly jump into a given repository with a file browser or command prompt. This is shown in the following gif.

![Navigation](https://raw.githubusercontent.com/awaescher/RepoZ/master/_doc/QuickNavigation.gif)

For Windows, use the hotkeys <kbd>Ctrl</kbd>+<kbd>Alt</kbd>+<kbd>R</kbd> to show RepoM.

<!--
To open a file browser, simply press <kbd>Return</kbd> on the keyboard once you selected a repository. To open a command prompt instead, hold <kbd>Ctrl</kbd> on Windows while pressing <kbd>Return</kbd>. These modifier keys will also work with mouse navigation.
-->
## Context Menu

The main functionality of RepoM are the quick actions to execute per repository. For instance, you can quickly naviate to the repository by directly opening the windows explorer or by opening a command prompt. This context menu is user and repostirory specific and can be defined using yaml. This way, you can add an context menu item (action) for opening the repository in Visual Studio (for a C# project) and for an other repository you can add the action to open a repository in Eclipse.

These actions are defined in the `RepositoryActions.yaml` located in your `%APPDATA%\RepoM\` folder. More information can be found [here](docs/RepositoryActions.md).

## Tagging

The `RepositoryActions.yaml` file also contains a section to define tags per repository on which you can filter and search when using the hub. How to define tags is described in the [RepositoryActions](docs/RepositoryActions.md) documentation.

## Search

[Search](docs/Search.md)

## Ordering and Filtering

[Filtering](docs/Filtering.md) [Ordering](docs/Ordering.md)

## Global configuration

 [Global settings](docs/Settings.md)

## Plugins

RepoM uses plugins to extend functionality. At this moment, when a plugin is available in the installed directory, it will be loaded. In the future, hopefully, a menu item in RepoM will be added so enable/disable plugins.

- [Plugins](docs/Plugins.md)
  - [AzureDevOps](docs/RepoM.Plugin.AzureDevOps.md)
  - [Clipboard](docs/RepoM.Plugin.Clipboard.md)
  - [EverythingFileSearch](docs/RepoM.Plugin.EverythingFileSearch.md)
  - [Heidi](docs/RepoM.Plugin.Heidi.md)
  - [LuceneQueryParser](docs/RepoM.Plugin.LuceneQueryParser.md)
  - [SonarCloud](docs/RepoM.Plugin.SonarCloud.md)
  - [Statistics](docs/RepoM.Plugin.Statistics.md)
  - [WindowsExplorerGitInfo](docs/RepoM.Plugin.WindowsExplorerGitInfo.md)
