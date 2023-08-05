Action to excute an application with additional arguments. This action is almost identical to the `command@1` action. When no existing executables are provided, the action will not show.

Action specific properties:

- `executables`: Set of possible executables. The first executable that exists will be used. The paths should absolute. (required, evaluated, string)
- `arguments`: Arguments for the executable. (optional, evaluated, string)
