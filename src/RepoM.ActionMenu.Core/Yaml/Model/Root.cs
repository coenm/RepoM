namespace RepoM.ActionMenu.Core.Yaml.Model;

using RepoM.ActionMenu.Core.Yaml.Model.ActionMenus;

public class Root : ContextRoot
{
    public Tags.Tags? Tags { get; set; }

    public ActionMenu? ActionMenu { get; set; }
}