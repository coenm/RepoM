using RepoM.Core.Plugin.AssemblyInformation;

[assembly: Package(
    "WindowsExplorerTitle",
    "Contains a hook updating Explorer views in Windows with the current git status.",
    "As an extra goodie for Windows users, RepoM automatically detects open File Explorer windows and adds a status appendix to their title if they are in context of a git repository.")]