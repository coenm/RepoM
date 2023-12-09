using RepoM.Core.Plugin.AssemblyInformation;

[assembly: Package(
    "HeidiSQL",
    "Contains variable provider using a HeidiDB configuration file as source. It also contains an action provider to open a database in HeidiSQL.",
    "This module integrates with a portable [HeidiSQL](https://www.heidisql.com/)  installation. The portable Heidi DB saves its database configuration in a portable configuration file. This module monitors this file and provides an action menu and a variable provider to access this information.")]

