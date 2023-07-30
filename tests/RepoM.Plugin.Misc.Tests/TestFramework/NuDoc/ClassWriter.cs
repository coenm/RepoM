namespace RepoM.Plugin.Misc.Tests.TestFramework.NuDoc;

using System.IO;

internal class ClassWriter
{
    public ClassWriter()
    {
        Head = new StringWriter();
        Properties = new StringWriter();
    }

    public readonly StringWriter Head;

    public readonly StringWriter Properties;
}