# RepoM

RepoM is a minimal-conf git repository hub with Windows Explorer enhancements. It uses the git repositories on your machine to create an efficient navigation widget and makes sure you'll never lose track of your work along the way.

It's populating itself as you work with git. It does not get in the way and does not require any user attention to work.

RepoM will not compete with your favourite git clients, so keep them. It's not about working within a repository: It's a new way to use all of your repositories to make your daily work easier.

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

## Search

To search for repositories, you can use a repom-dialect of Lucene query syntax.

> ### Terms
>
> A query is broken up into terms and operators. There are two types of terms: Single Terms and Phrases.
>
> A Single Term is a single word such as "test" or "hello".
>
> A Phrase is a group of words surrounded by double quotes such as "hello dolly".
>
> Multiple terms can be combined together with Boolean operators to form a more complex query.
>
> ### Fields
>
> Lucene supports fielded data. When performing a search you can either specify a field, or use the default field. The field names and default field is implementation specific.
>
> You can search any field by typing the field name followed by a colon `:` and then the term you are looking for.
>
> As an example, let's assume a Lucene index contains two fields, title and text and text is the default field. If you want to find the document entitled "The Right Way" which contains the text "don't go this way", you can enter:
>
> `title:"The Right Way" AND text:go`
>
> or
>
> `title:"Do it right" AND right`
>
> Since text is the default field, the field indicator is not required.
>
> Note: The field is only valid for the term that it directly precedes, so the query
>
> `title:Do it right`
>
> Will only find "Do" in the title field. It will find "it" and "right" in the default field (in this case the text field).
>
> Note: The analyzer used to create the index will be used on the terms and phrases in the query string. So it is important to choose an analyzer that will not interfere with the terms used in the query string.
>
> ### Boolean Operators
>
> Boolean operators allow terms to be combined through logic operators. Lucene supports `AND`, `OR`, `NOT` and `-` as Boolean operators (Note: Boolean operators must be ALL CAPS).
>
>The `AND` operator is the default conjunction operator. This means that if there is no Boolean operator between two terms, the `AND` operator is used.
>
> The `AND` operator links two terms and finds repositories when both terms are a match.
> The `OR` operator finds repositories if either of the terms matches in a repository.
>
> To search for documents that contain either "jakarta apache" or just "jakarta" use the query:
>
> `"jakarta apache" OR jakarta`

## Plugin: Enhanced Windows Explorer Titles

As an extra goodie for Windows users, RepoZ automatically detects open File Explorer windows and adds a status appendix to their title if they are in context of a git repository.

![Screenshot](https://raw.githubusercontent.com/awaescher/RepoZ/master/_doc/RepoZ-ReadMe-Explorer.png)

## Credits

RepoM is a fork of the amazing RepoZ, which was created by [Andreas Wäscher](https://github.com/awaescher/RepoZ).
