namespace RepoM.Api.Common.Tests.IO.ModuleBasedRepositoryActionProvider;

using EasyTestFile;

public static class EasyTestFileSettingsExtensions
{
    public static EasyTestFileSettings SetExtension(this EasyTestFileSettings @this, SerializationType type)
    {
        var result = new EasyTestFileSettings(@this);
        switch (type)
        {
            case SerializationType.Json:
                result.UseExtension("json");
                break;
            case SerializationType.Yaml:
                result.UseExtension("yaml");
                break;
            default:
                // do nothing
                break;
        }

        return result;
    }
}