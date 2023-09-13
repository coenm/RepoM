# Settings

When RepoM starts for the first time, a configuration file wil be created. Most of the properties can be adjusted using the UI but, at this moment, one property must be altered manually.

To do this, make sure RepoM is not running.

## Configuration <!-- include: DocsAppSettingsTests.AppSettingsJsonFileGeneration.verified.md -->

This is the default configuration.

```json
{
  "SortKey": "",
  "SelectedQueryParser": "",
  "SelectedFilter": "",
  "AutoFetchMode": 0,
  "PruneOnFetch": false,
  "MenuSize": {
    "Height": -1.0,
    "Width": -1.0
  },
  "ReposRootDirectories": [],
  "EnabledSearchProviders": [],
  "Plugins": []
}
```
<!-- endInclude -->

Properties: <!-- include: DocsAppSettingsTests.AppSettingsDocumentationGeneration_AppSettings.verified.md -->

- `SortKey`: The selected sorting strategy. Sorting strategies can be configured manually in `RepoM.Ordering.yaml`. (optional, UI configured)
- `SelectedQueryParser`: The selected query parser. Query parsers can be added by plugins. (optional, UI configured)
- `SelectedFilter`: The selected filtering strategy. Filtering strategies can be configured manually in `RepoM.Filtering.yaml`. (optional, UI configured)
- `AutoFetchMode`: The git fetching strategy. This determines how often RepoM will fetch from git. (optional, UI configured)
- `PruneOnFetch`: This option determines if RepoM should prune branches when fetching from git. (optional, UI configured)
- `MenuSize`: The menu size of RepoM. This is set when the window is resized. (optional, UI configured)
- `ReposRootDirectories`: List of root directories where RepoM will search for git repositories. If null or empty, all fixed drives will be searched from the root. (optional, Manual configured)
- `EnabledSearchProviders`: List of search providers. Search providers can be added by plugins. (optional, UI configured)
- `Plugins`: List of plugins. (optional, UI configured) <!-- endInclude -->
