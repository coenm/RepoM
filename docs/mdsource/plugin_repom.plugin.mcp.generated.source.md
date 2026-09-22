# Mcp

This module starts an MCP (Model Context Protocol) server that exposes repository information through MCP tools. AI assistants and other MCP clients can connect to query repository data such as branches, status, and remotes.

To use this module, make sure it is enabled in RepoM by opening the menu and navigate to 'Plugins'. After enabling or disabling a plugin, you should restart RepoM.

## Configuration

This plugin has specific configuration stored in the following directory `%APPDATA%/RepoM/Module/`. This configuration file should be edit manually. The safest way to do this is, is when RepoM is not running.

The following default configuration is used:

```json
{
  "Version": 1,
  "Settings": {
    "Port": 17823
  }
}
```

### Properties

- `Port`: The port number on which the MCP server listens for HTTP connections. Defaults to 17823.

## Connecting to the MCP server

Once the plugin is enabled and RepoM is running, an MCP server will be available at `http://localhost:17823/mcp`. You can connect to this server using any MCP-compatible client.

### VS Code (GitHub Copilot)

Add the following to your `.vscode/mcp.json` or user settings:

```json
{
  "servers": {
    "repom": {
      "type": "http",
      "url": "http://localhost:17823/mcp"
    }
  }
}
```

## Available MCP tools

The following MCP tools are exposed by this plugin:

### list_repositories

Lists all tracked repositories. Optionally filter by name.

Parameters:

- `nameFilter` (optional): Filter to match repository names (case-insensitive, partial match).

### get_repository

Gets detailed information about a specific repository by its path. Returns branch information, remotes, tags, and detailed change status.

Parameters:

- `path` (required): The full file system path of the repository.

### find_repositories

Searches for repositories matching specific criteria such as branch, status, or remote URL.

Parameters:

- `branch` (optional): Filter by current branch name (case-insensitive, partial match).
- `hasLocalChanges` (optional): When true, only return repositories with local uncommitted changes.
- `isBehind` (optional): When true, only return repositories that are behind their remote.
- `hasUnpushedChanges` (optional): When true, only return repositories with unpushed commits or changes.
- `remoteUrl` (optional): Filter by remote URL (case-insensitive, partial match).
