# AzureDevOps

The AzureDevops module enables integration with one azure devops environment. The integration currently focusses on Pull Requests.

To use this module, make sure it is enabled in RepoM by opening the menu and navigate to 'Plugins'. When enabling or disabling a plugin, you should restart RepoM.

This plugin has specific configuration stored in a seperate configuration file placed in the `%APPDATA%/RepoM/Module/` directory. This configuration file should be edit manually. The safest way to do this is, is when RepoM is not running.

Provide a `PersonalAccessToken` and a `BaseUrl`. The `BaseUrl` must end with the organisation and a slash (ie, `https://dev.azure.com/organisation/`) and the 'PAT' should be granted access to `XYZ` (TODO).

include: _plugins.azuredevops.action
