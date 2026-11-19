namespace RepoM.App.Tests.TestFramework;

using System.Runtime.CompilerServices;
using VerifyTests;
using VerifyXunit;

public static class VerifierInitializer
{
    [ModuleInitializer]
    public static void Initialize()
    {
        VerifyXaml.Initialize();
        // VerifierSettings.RegisterStreamComparer(
        //     extension: "png",
        //     compare: XamlPngComparer.CompareAsync);
    }
}