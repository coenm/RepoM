# HeidiSQL

This module integrates with a portable [HeidiSQL](https://www.heidisql.com/)  installation. The portable Heidi DB saves its database configuration in a portable configuration file. This module monitors this file and provides an action menu and a variable provider to access this information.

To use this module, make sure it is enabled in RepoM by opening the menu and navigate to 'Plugins'. When enabling or disabling a plugin, you should restart RepoM.

ProjectName: RepoM.Plugin.Heidi
PluginName: HeidiSQL
PluginDescription: Contains variable provider using a HeidiDB configuration file as source. It also contains an action provider to open a database in HeidiSQL.
PluginMarkdownDescription: This module integrates with a portable [HeidiSQL](https://www.heidisql.com/)  installation. The portable Heidi DB saves its database configuration in a portable configuration file. This module monitors this file and provides an action menu and a variable provider to access this information.

This module contains the following methods, variables and/or constants:

[## `heidi-databases@1`](#heidi-databases@1)

Action to list heidi databases and show action menus for them.

Action specific properties:
- `key` (RepoM.ActionMenu.Interface.YamlModel.Templating.Text): Repository key.
If not provided, the repository `Remote.Origin.Name` is used as selector.
- `executable` (RepoM.ActionMenu.Interface.YamlModel.Templating.Text): The absolute path of the Heidi executable. If not provided, the default value from the plugin settings is used.
- `context` (RepoM.ActionMenu.Interface.YamlModel.ActionMenus.Context?): 
- `active` (RepoM.ActionMenu.Interface.YamlModel.Templating.Predicate): 
[## `heidi-databases@1`](#heidi-databases@1)

Action to list heidi databases and show action menus for them.

Action specific properties:
- `key` (string?): Repository key.
If not provided, the repository `Remote.Origin.Name` is used as selector.
- `executable` (string?): The absolute path of the Heidi executable. If not provided, the default value from the plugin settings is used.
