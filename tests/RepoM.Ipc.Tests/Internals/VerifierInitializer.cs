namespace RepoM.Ipc.Tests.Internals;

using System.Runtime.CompilerServices;
using Newtonsoft.Json;
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