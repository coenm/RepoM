namespace RepoM.Plugin.SonarCloud.ActionMenu.Context;

using System;
using JetBrains.Annotations;
using RepoM.ActionMenu.Interface.Attributes;
using RepoM.ActionMenu.Interface.Scriban;
using RepoM.Plugin.SonarCloud;

[UsedImplicitly]
[ActionMenuModule("sonarcloud")]
internal partial class SonarCloudVariables : TemplateContextRegistrationBase
{
    private readonly ISonarCloudFavoriteService _service;

    public SonarCloudVariables(ISonarCloudFavoriteService service)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
    }

    [ActionMenuMember("is_favorite")]
    public bool IsFavorite(string key)
    {
        return _service.IsFavorite(key ?? string.Empty);
    }
}