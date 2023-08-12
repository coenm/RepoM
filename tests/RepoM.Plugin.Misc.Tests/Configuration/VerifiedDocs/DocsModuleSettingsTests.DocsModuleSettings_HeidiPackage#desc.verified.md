## Configuration

This plugin has specific configuration stored in a separate configuration file stored in `%APPDATA%/RepoM/Module/` directory. This configuration file should be edit manually. The safest way to do this is, is when RepoM is not running.

The following default configuration is used:

```json
{
  "Version": 1,
  "Settings": {
    "ConfigPath": null,
    "ConfigFilename": null,
    "ExecutableFilename": null
  }
}
```

Properties:

- `ConfigPath`: The full directory where the portable configuration file is stored.
- `ConfigFilename`: The portable-configurration filename (without path). Most likely `portable_settings.txt`
- `ExecutableFilename`: The full executable of Heidi.
