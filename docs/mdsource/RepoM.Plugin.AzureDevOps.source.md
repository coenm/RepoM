# AzureDevOps

The AzureDevops module enables integration with one azure devops environment. The integration currently focusses on Pull Requests.

To enable the AzureDevops module, manually update the `appsettings.json` file and provide for the `AzureDevOps`section both a `PersonalAccessToken` and `BaseUrl`. The `BaseUrl` must end with the organisation and a slash (ie, `https://dev.azure.com/organisation/`).

include: _plugins.azuredevops.action
