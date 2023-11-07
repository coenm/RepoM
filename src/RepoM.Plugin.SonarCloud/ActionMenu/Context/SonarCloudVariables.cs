namespace RepoM.Plugin.SonarCloud.ActionMenu.Context;

using System;
using JetBrains.Annotations;
using RepoM.ActionMenu.Interface.Attributes;
using RepoM.ActionMenu.Interface.Scriban;
using RepoM.Plugin.SonarCloud;

/// <summary>
/// Provides a sonar cloud method providing the favorite status of the current repository.
/// </summary>
[UsedImplicitly]
[ActionMenuContext("sonarcloud")]
internal partial class SonarCloudVariables : TemplateContextRegistrationBase
{
    private readonly ISonarCloudFavoriteService _service;

    public SonarCloudVariables(ISonarCloudFavoriteService service)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
    }

    /// <summary>
    /// Get favorite status of repository related to the <paramref name="id"/>.
    /// </summary>
    /// <param name="id">The sonarcloud id related to the repository.</param>
    /// <returns>`true` when the repository is set as favorite in SonarCloud, `false`, otherwise.</returns>
    /// <example>
    /// <code>
    /// sonarcloud_repository_id = "RepoM";
    /// is_favorite = sonarcloud.is_favorite(sonarcloud_repository_id);
    /// </code>
    /// </example>
    [ActionMenuContextMember("is_favorite")]
    public bool IsFavorite(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return false;
        }

        return _service.IsFavorite(id);
    }
}