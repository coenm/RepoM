namespace Yaml.Poc.Tests.TestFramework;

using System.Runtime.CompilerServices;
using Argon;
using VerifyTests;

public static class VerifierInitializer
{
    [ModuleInitializer]
    public static void Initialize()
    {
        VerifierSettings.DisableRequireUniquePrefix();
        VerifierSettings.AddExtraSettings(serializerSettings => serializerSettings.TypeNameHandling = TypeNameHandling.Auto);
    }
}