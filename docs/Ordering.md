# Ordering

The order of the repositories shown in RepoM is customizable. The default is alphabetical on the repository name.

Orderings are defined in yaml using repository-`comparers` and repository-`scorers` and you can switch between different orderings at any given time using the hamburger menu in RepoM.

## Comparers

### az-comparer@1<!-- include: RepositoriesComparerConfigurationTests.CoreComparersMarkdown.verified.md -->

Compares two repositories by a given property.<!-- include: RepositoriesComparerConfigurationTests.DocsRepositoriesComparerConfiguration_AlphabetComparerConfigurationV1.verified.md -->

Comparer specific properties:

- `property`: Repository property. Currently, only `Name`, and `Location` are supported. Otherwise, comparison will always result in `0`. (optional)
- `weight`: The weight of this comparer. The higher the weight, the higher the impact.<!-- endInclude -->

### composition@1

Compares two repositories by a composition of comparers.<!-- include: RepositoriesComparerConfigurationTests.DocsRepositoriesComparerConfiguration_CompositionComparerConfigurationV1.verified.md -->

Comparer specific properties:

- `comparers`: List of comparers. The first comparer not resulting in `0` will be used as final result.<!-- endInclude -->

### score-comparer@1

Compares two repositories by a repository score. The calculation of the repository score is defined in the score provider.<!-- include: RepositoriesComparerConfigurationTests.DocsRepositoriesComparerConfiguration_ScoreComparerConfigurationV1.verified.md -->

Comparer specific properties:

- `score-provider`: The score provider to calculate a score for a repository. (optional)<!-- endInclude -->

### sum-comparer@1

Compares two repositories by the sum of the results of the comparers.<!-- include: RepositoriesComparerConfigurationTests.DocsRepositoriesComparerConfiguration_SumComparerConfigurationV1.verified.md -->

Comparer specific properties:

- `comparers`: A list of comparers. The sum of the results of the comparers will be used as final result.<!-- endInclude -->
<!-- endInclude -->
