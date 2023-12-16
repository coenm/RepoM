# HeidiSQL

This module integrates with a portable [HeidiSQL](https://www.heidisql.com/)  installation. The portable Heidi DB saves its database configuration in a portable configuration file. This module monitors this file and provides an action menu and a variable provider to access this information.

To use this module, make sure it is enabled in RepoM by opening the menu and navigate to 'Plugins'. When enabling or disabling a plugin, you should restart RepoM.

- ProjectName: RepoM.Plugin.Heidi
- PluginName: HeidiSQL
- PluginDescription: Contains variable provider using a HeidiDB configuration file as source. It also contains an action provider to open a database in HeidiSQL.
- PluginMarkdownDescription: This module integrates with a portable [HeidiSQL](https://www.heidisql.com/)  installation. The portable Heidi DB saves its database configuration in a portable configuration file. This module monitors this file and provides an action menu and a variable provider to access this information.

This module contains the following methods, variables and/or constants:

## heidi-databases@1

Action to list heidi databases and show action menus for them.

Properties:

- `key`: Repository key.
If not provided, the repository `Remote.Origin.Name` is used as selector. ([Text](https://this-is.com/Text))
- `executable`: The absolute path of the Heidi executable. If not provided, the default value from the plugin settings is used. ([Text](https://this-is.com/Text))
- `context`:  ([Context](https://this-is.com/Context))
- `active`:  ([Predicate](https://this-is.com/Predicate))
