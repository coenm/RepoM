This plugin has specific configuration stored in a separate configuration file stored in `%APPDATA%/RepoM/Module/` directory. This configuration file should be edit manually. The safest way to do this is, is when RepoM is not running.

The following default configuration is used

```json
{
  "Version": 1,
  "Settings": {
    "PersonalAccessToken": null,
    "BaseUrl": null
  }
}
```

Properties:

- `PersonalAccessToken`: Personal access token (PAT) to access Azure Devops. The PAT should be granted access to `todo` rights
- `BaseUrl`: The base url of azure devops for your organisation (ie. `https://dev.azure.com/my-organisation/`).
