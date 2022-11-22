# RepoM

RepoM is a minimal-conf git repository hub with Windows Explorer enhancements. It uses the git repositories on your machine to create an efficient navigation widget and makes sure you'll never lose track of your work along the way.

It's populating itself as you work with git. It does not get in the way and does not require any user attention to work.

Repo< will not compete with your favourite git clients, so keep them. It's not about working within a repository: It's a new way to use all of your repositories to make your daily work easier.

📦  [Check the Releases page](https://github.com/coenm/RepoM/releases) to **download** the latest version and see **what's new**!

## The Hub

The hub provides a quick overview of your repositories including their current branch and a short status information. Additionally, it offers some shortcuts like revealing a repository in the Windows Explorer, opening a command line tool in a given repository and checking out git branches.

![Screenshot](https://raw.githubusercontent.com/awaescher/RepoZ/master/_doc/RepoZ-ReadMe-UI-Both.png)

> **"Well ok, that's a neat summary ..."** you might say **"... but how does this help?"**.

If you are working on different git repositories throughout the day, you might find yourself wasting time by permanently switching over from one repository to another. If you are like me, you tend to keep all those windows open to be reused later, ending up on a window list which has to be looped through all the time.

With RepoZ, you can instantly jump into a given repository with a file browser or command prompt. This is shown in the following gif.

![Navigation](https://raw.githubusercontent.com/awaescher/RepoZ/master/_doc/QuickNavigation.gif)

For Windows, use the hotkeys <kbd>Ctrl</kbd>+<kbd>Alt</kbd>+<kbd>R</kbd> to show RepoM.

To open a file browser, simply press <kbd>Return</kbd> on the keyboard once you selected a repository. To open a command prompt instead, hold <kbd>Ctrl</kbd> on Windows or <kbd>Command</kbd> on macOS while pressing <kbd>Return</kbd>. These modifier keys will also work with mouse navigation.

## Enhanced Windows Explorer Titles

As an extra goodie for Windows users, RepoZ automatically detects open File Explorer windows and adds a status appendix to their title if they are in context of a git repository.

![Screenshot](https://raw.githubusercontent.com/awaescher/RepoZ/master/_doc/RepoZ-ReadMe-Explorer.png)

## Credits

RepoM is a fork of the amazing RepoZ, which was created by [Andreas Wäscher](https://github.com/awaescher/RepoZ).
