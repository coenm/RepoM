Action to pin (or unpin) the current repository. Pinning is not persistant and all pinned repositories will be cleared when RepoM exits.
Pinning a repository allowed custom filtering, ordering and searching.

Action specific properties:

- `name`: Name of the action. This is shown in the UI of RepoM. When no value is provided, the name will be a default value based on the mode. (optional, evaluated, string)
- `mode`: The pin mode `[Toggle, Pin, UnPin]`. (required, pinmode)
