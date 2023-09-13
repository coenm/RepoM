## Configuration

This plugin has specific configuration stored in a separate configuration file stored in `%APPDATA%/RepoM/Module/` directory. This configuration file should be edit manually. The safest way to do this is, is when RepoM is not running.

The following default configuration is used:

```json
{
  "Version": 1,
  "Settings": {
    "Browsers": {},
    "Profiles": {}
  }
}
```

Properties:

- `Browsers`: Dictionary of known browsers and their path to use for opening urls.
- `Profiles`: Profiles to use. 
