namespace RepoM.Plugin.Misc.Tests.TestFramework.NuDoc;

using System.IO;

internal class ClassWriter
{
    public readonly StringWriter Head = new();

    public readonly StringWriter Properties = new();
}