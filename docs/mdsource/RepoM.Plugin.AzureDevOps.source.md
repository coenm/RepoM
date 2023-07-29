# AzureDevOps

The AzureDevops module enables integration with one azure devops environment. The integration currently focusses on Pull Requests.

include: _plugin_enable

include: DocsModuleSettingsTests.DocsModuleSettings_AzureDevOpsPackage#desc.verified.md

Provide a `PersonalAccessToken` and a `BaseUrl`. The `BaseUrl` must end with the organisation and a slash (ie, `https://dev.azure.com/organisation/`) and the 'PAT' should be granted access to `XYZ` (TODO).

include: _plugins.azuredevops.action
