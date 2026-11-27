namespace RepoM.StartupPlugin.Demo.Tests.TestFramework;

using System.Runtime.CompilerServices;
using VerifyTests;
using VerifyXunit;

public static class VerifierInitializer
{
    [ModuleInitializer]
    public static void Initialize()
    {
        VerifyXaml.Initialize();
        VerifyPhash.Initialize();
        VerifyPhash.RegisterComparer("png", threshold: 0.80f);
        VerifierSettings.RegisterStringComparer("xml", OrderInsensitiveXmlComparer.CompareAsync);
        VerifierSettings.RegisterStreamComparer(
            extension: "gif",
            compare: AnimatedGifComparer.CompareAsync);
    }
}
