## sonarcloud-set-favorite@1

Action to mark a repository as favorite within SonarCloud. This action requires the use of the SonarCloud plugin.

Custom properties:

- Name: The name of the item (required, string, evaulated)
- Enabled: If the action is clickable (optional, boolean/string, evaluated, default true)
- Project: The SonarCloud project key (required, string, evaluated)

include: DocsRepositoryActionsTests.DocsRepositoryActionsSettings_RepositoryActionSonarCloudSetFavoriteV1.verified.md

Example:

snippet: RepositoryActionsSonarCloudSetFavorite01
