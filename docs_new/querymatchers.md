# QueryMatchers

RepoM uses query matchers to check if a given term matches a given repository.

The following query matchers are currently implemented:

| Matcher type              | Term types                 | Term | Accepted values                        | Description                                                                                |
|---------------------------|----------------------------|------|----------------------------------------|--------------------------------------------------------------------------------------------|
| FreeTextMatcher           | FreeText                   |      | *                                      | Checks if the free text matches a tag, else if the repositiry name contains the free text. |
| NameMatcher               | SimpleTerm, StartsWithTerm | name | *                                      | Checks if the repository name equals the given name                                        |
| TagMatcher                | SimpleTerm, StartsWithTerm | tag  | *                                      | Checks if the repository tags contain the given tag                                        |
| IsPinnedMatcher           | SimpleTerm                 | is   | pinned                                 | Checks if a repository is pinned or not                                                    |
| IsBareRepositoryMatcher   | SimpleTerm                 | is   | bare                                   | Returns true if a repository is a bare repository.                                         |
| IsBehindMatcher           | SimpleTerm                 | is   | behind                                 | Returns true if a repository has pending updates from the remote.                          |
| HasUnPushedChangesMatcher | SimpleTerm                 | has  | changes                                | Returns true if there are changed uncommitted files.                                       |
| HasPullRequestsMatcher    | SimpleTerm                 | has  | [pr, prs, pull-request, pull-requests] | Returns true if there are known pull requests available in Azure DevOps.                   |