This action results in zero or more items in the contextmenu. For each open pullrequest for the given repository, it will show an action to go to the specific PullRequest in your favorite webbrowser.

Action specific properties:

- `project-id`: The azure devops project id. (required, evaluated, string)
- `repository-id`: The repository Id. If not provided, the repository id is located using the remote url. (optional, evaluated, string)
- `show-when-empty`: When no pull requests are available, this property is used to determine if no or a message item is showed. (optional, evaluated, string, default: `true`)
