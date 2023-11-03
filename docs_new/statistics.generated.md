# `statistics`

Provides statistical information accessible through `statistics`.

This module contains the following methods, variables and/or constants:

- [`statistics.count`](#statistics-count)
- [`statistics.overall_count`](#statistics-overall-count)

## count

`statistics.count`

Gets the number of actions performed on the current repository.

### Returns

Number of actions performed on the current repository.

### Example
      
repo_call_count = statistics.count;

## overall_count

`statistics.overall_count`

Gets the number of actions performed on all repositories known in RepoM.

### Returns

Number of actions performed on any known repository.

### Example
      
total_count = statistics.overall_count;
