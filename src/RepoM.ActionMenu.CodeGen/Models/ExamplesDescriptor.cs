namespace RepoM.ActionMenu.CodeGen.Models;

using System.Collections.Generic;

public class ExamplesDescriptor
{
    public string? Description { get; set; }

    public List<ExampleItemBase> Items { get; } = [];
}

public abstract class ExampleItemBase
{
    public abstract string TypeName { get; }
}

public sealed class Code : ExampleItemBase
{
    public override string TypeName { get; } = nameof(Code);

    public string? Language { get; set; } = null;

    public string Content { get; init; }

    public bool UseRaw { get; set; }
}

public sealed class Paragraph : ExampleItemBase
{
    public override string TypeName { get; } = nameof(Paragraph);

    public string Text { get; init; }
}

public sealed class Text : ExampleItemBase
{
    public override string TypeName { get; } = nameof(Text);

    public string Content { get; init; }
}

public sealed class Header : ExampleItemBase
{
    public override string TypeName { get; } = nameof(Header);

    public string Text { get; init; }
}

public sealed class Snippet : ExampleItemBase
{
    public override string TypeName { get; } = nameof(Snippet);

    public SnippetMode Mode { get; set; } = SnippetMode.Include;

    public string? Language { get; set; } = null;

    public string Name { get; init; }
}

public enum SnippetMode
{
    Include,

    Snippet,
}