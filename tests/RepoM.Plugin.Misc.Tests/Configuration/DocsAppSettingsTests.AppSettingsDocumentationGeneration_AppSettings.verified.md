Properties:

- `SortKey`: The selected sorting strategy. Sorting strategies can be configured manually in `RepoM.Ordering.yaml`. (optional, UI configured)
- `SelectedQueryParser`: The selected query parser. Query parsers can be added by plugins. (optional, UI configured)
- `SelectedFilter`: The selected filtering strategy. Filtering strategies can be configured manually in `RepoM.Filtering.yaml`. (optional, UI configured)
- `AutoFetchMode`: The git fetching strategy. This determines how often RepoM will fetch from git. (optional, UI configured)
- `PruneOnFetch`: This option determines if RepoM should prune branches when fetching from git. (optional, UI configured)
- `MenuSize`: The menu size of RepoM. This is set when the window is resized. (optional, UI configured)
- `ReposRootDirectories`: List of root directories where RepoM will search for git repositories. If null or empty, all fixed drives will be searched from the root. (optional, Manual configured)
- `EnabledSearchProviders`: List of search providers. Search providers can be added by plugins. (optional, UI configured)
- `SonarCloudPersonalAccessToken`: SonarCloud PAT. (<span style="color: red;">Obsolete, moved to plugin config</span>)
- `AzureDevOps`: Azure DevOps config. (<span style="color: red;">Obsolete, moved to plugin config</span>)
- `Plugins`: List of plugins. (optional, UI configured)
