# Ordering

The order of the repositories shown in RepoM is customizable. The default is alphabetical on the repository name.

Orderings are defined in yaml using repository-`comparers` and repository-`scorers`. You can switch at any given time between orderings using the hamburgermenu in RepoM.

## Definitions

A [`comparer`](#comparers) compares two repositories resulting in an order between the two. The comparison is defined by the type of comparer. It is possible to compare by repository name (assending order) or to compare repositories by the timestamp of the last action performed by RepoM.

[`Scorers`](#scorers) however calculate a score over exactly one repository. It does not compare repositories. Comparing repositories by score, use the `score-comparer@1`.

## Comparers

- [`az-comparer@1`](#az-comparer1)<!-- include: RepositoriesComparerConfigurationTests.CoreComparersMarkdown.verified.md -->
- [`composition-comparer@1`](#composition-comparer1)
- [`score-comparer@1`](#score-comparer1)
- [`sum-comparer@1`](#sum-comparer1)

These comparers are available by using the corresponding plugin.
- [`last-opened-comparer@1`](#last-opened-comparer1)

### `az-comparer@1`

Compares two repositories by a given property alphabetically in ascending order.<!-- include: RepositoriesComparerConfigurationTests.DocsRepositoriesComparerConfiguration_AlphabetComparerConfigurationV1.verified.md -->

Properties:

- `property`: Repository property. Currently, only `Name`, and `Location` are supported. Otherwise, comparison will always result in `0`. (optional)
- `weight`: The weight of this comparer. The higher the weight, the higher the impact.<!-- endInclude -->

### `composition-comparer@1`

Compares two repositories by a composition of comparers.<!-- include: RepositoriesComparerConfigurationTests.DocsRepositoriesComparerConfiguration_CompositionComparerConfigurationV1.verified.md -->

Properties:

- `comparers`: List of comparers. The first comparer not resulting in `0` will be used as final result.<!-- endInclude -->

### `score-comparer@1`

Compares two repositories by a repository score. The calculation of the repository score is defined in the score provider.<!-- include: RepositoriesComparerConfigurationTests.DocsRepositoriesComparerConfiguration_ScoreComparerConfigurationV1.verified.md -->

Properties:

- `score-provider`: The score provider to calculate a score for a repository. (optional)<!-- endInclude -->

### `sum-comparer@1`

Compares two repositories by the sum of the results of the comparers.<!-- include: RepositoriesComparerConfigurationTests.DocsRepositoriesComparerConfiguration_SumComparerConfigurationV1.verified.md -->

Properties:

- `comparers`: A list of comparers. The sum of the results of the comparers will be used as final result.<!-- endInclude -->

### `last-opened-comparer@1`

Compares two repositories by the timestamp of the last action RepoM performed on the repository.<!-- include: RepositoriesComparerConfigurationTests.DocsRepositoriesComparerConfiguration_LastOpenedConfigurationV1.verified.md -->

Properties:

- `weight`: The weight of this comparer. The higher the weight, the higher the impact.<!-- endInclude -->
<!-- endInclude -->

## Scorers

- [`is-pinned-scorer@1`](#is-pinned-scorer1)<!-- include: RepositoriesScorerConfigurationTests.CoreScorersMarkdown.verified.md -->
- [`tag-scorer@1`](#tag-scorer1)

These scorers are available by using the corresponding plugin.
- [`usage-scorer@1`](#usage-scorer1)

### `is-pinned-scorer@1`

Repository scorer based on the pinned state of a repository.<!-- include: RepositoriesScorerConfigurationTests.DocsRepositoriesScorerConfiguration_IsPinnedScorerConfigurationV1.verified.md -->

Properties:

- `weight`: The weight of this scorer. The higher the weight, the higher the impact.<!-- endInclude -->

### `tag-scorer@1`

Repository scorer based on the tags of a repository.<!-- include: RepositoriesScorerConfigurationTests.DocsRepositoriesScorerConfiguration_TagScorerConfigurationV1.verified.md -->

Properties:

- `weight`: The weight of this scorer. The higher the weight, the higher the impact.
- `tag`: The tag to match on. If the repository has this tag, the score will return the weight, otherwise, `0`. (optional)<!-- endInclude -->

### `usage-scorer@1`

Repository scorer based on it's usage by RepoM. The more it's used, the higher the score.<!-- include: RepositoriesScorerConfigurationTests.DocsRepositoriesScorerConfiguration_UsageScorerConfigurationV1.verified.md -->

Properties:

- `windows`: Specific 'windows' to calculate the score for. 
- `max-score`: The maximum score a repository can get.<!-- endInclude -->
<!-- endInclude -->
