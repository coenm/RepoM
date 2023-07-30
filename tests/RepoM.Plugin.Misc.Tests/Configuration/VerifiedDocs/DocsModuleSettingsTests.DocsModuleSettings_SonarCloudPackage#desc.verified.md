﻿## Configuration

This plugin has specific configuration stored in a separate configuration file stored in `%APPDATA%/RepoM/Module/` directory. This configuration file should be edit manually. The safest way to do this is, is when RepoM is not running.

The following default configuration is used:

```json
{
  "Version": 1,
  "Settings": {
    "PersonalAccessToken": null,
    "BaseUrl": "https://sonarcloud.io"
  }
}
```

Properties:

- `PersonalAccessToken`: Personal Access Token to access SonarCloud.
- `BaseUrl`: SonarCloud url. Most likely `https//sonarcloud.io`.
