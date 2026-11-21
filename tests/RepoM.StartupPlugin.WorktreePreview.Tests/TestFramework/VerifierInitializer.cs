namespace RepoM.StartupPlugin.WorktreePreview.Tests.TestFramework;

using System.Runtime.CompilerServices;
using VerifyTests;
using VerifyXunit;

public static class VerifierInitializer
{
    [ModuleInitializer]
    public static void Initialize()
    {
        VerifyXaml.Initialize();
        VerifierSettings.RegisterStringComparer("xml", OrderInsensitiveXmlComparer.CompareAsync);
        VerifierSettings.RegisterStreamComparer(
            extension: "png",
            compare: XamlPngComparer.CompareAsync);
    }
}
