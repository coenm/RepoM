## Configuration

This plugin has specific configuration stored in a separate configuration file stored in `%APPDATA%/RepoM/Module/` directory. This configuration file should be edit manually. The safest way to do this is, is when RepoM is not running.

The following default configuration is used:

```json
{
  "Version": 1,
  "Settings": {
    "PersistenceBuffer": "00:05:00",
    "RetentionDays": 30
  }
}
```

Properties:

- `PersistenceBuffer`: Timespan for buffered events before making them persistant (i.e. `00:05:00` for five minutes). Must be greater then or equal to `00:00:10` (10 seconds).
- `RetentionDays`: Number of days to keep statical information before deleting them. 
