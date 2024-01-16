## Configuration

This plugin has specific configuration stored in a separate configuration file stored in `%APPDATA%/RepoM/Module/` directory. This configuration file should be edit manually. The safest way to do this is, is when RepoM is not running.

The following default configuration is used:

```json
{
  "Version": 2,
  "Settings": {
    "PersonalAccessToken": null,
    "BaseUrl": null,
    "DefaultProjectId": null,
    "IntervalUpdatePullRequests": "00:04:00",
    "IntervalUpdateProjects": "00:10:00"
  }
}
```

Properties:

- `PersonalAccessToken`: Personal access token (PAT) to access Azure Devops. The PAT should be granted access to `todo` rights.
To create a PAT, goto `https://dev.azure.com/[my-organisation]/_usersSettings/tokens`.
- `BaseUrl`: The base url of azure devops for your organisation (ie. `https://dev.azure.com/[my-organisation]/`).
- `DefaultProjectId`: Default project id to use when no project id provided in the repository action. Should be a GUID.
- `IntervalUpdatePullRequests`: Interval RepoM should update the list of open pull requests from Azure DevOps. Defaults to `4` minutes (ie. `00:04:00`).
- `IntervalUpdateProjects`: Interval RepoM should update the list of projects from Azure DevOps. Defaults to `10` minutes (ie. `00:10:00`).
