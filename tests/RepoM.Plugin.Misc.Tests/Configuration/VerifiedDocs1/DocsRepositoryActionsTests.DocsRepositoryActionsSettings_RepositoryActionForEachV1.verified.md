Action to create repeated actions based on a variable.

Action specific properties:

- `enumerable`: The list of items to enumerate on. (required, evaluated, string)
- `variable`: The name of the variable to access to current enumeration of the  items. For each iteration, the variable `{var.name}` has the value of the current iteration. (required, evaluated, string)
- `skip`: Predicate to skip the current item. (optional, evaluated, string)
- `actions`: List of repeated actions. (required, evaluated, ienumerable`1)
