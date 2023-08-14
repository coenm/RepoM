# Ordering

The order of the repositories shown in RepoM is customizable. The default is alphabetical on the repository name.

Orderings are defined in yaml using repository-`comparers` and repository-`scorers`. You can switch at any given time between orderings using the hamburgermenu in RepoM.

## Definitions

A `comparer` compares two repositories resulting in an order between the two. The comparison is defined by the type of comparer. It is possible to compare by repository name (assending order) or to compare repositories by the timestamp of the last action performed by RepoM.

`Scorers` however calculate a score over exactly one repository. It does not compare repositories. Comparing repositories by score, use the `score-comparer@1`.

## Comparers

include: RepositoriesComparerConfigurationTests.CoreComparersMarkdown.verified.md

## Scorers

include: RepositoriesScorerConfigurationTests.CoreScorersMarkdown.verified.md

