namespace RepoM.Api.Git;

using System;

public class TagViewModel
{
    public TagViewModel(string tag)
    {
        Tag = tag ?? throw new ArgumentNullException(nameof(tag));
    }

    public string Tag { get; }
}