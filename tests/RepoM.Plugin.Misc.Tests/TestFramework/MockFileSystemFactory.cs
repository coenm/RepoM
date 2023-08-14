namespace RepoM.Plugin.Misc.Tests.TestFramework;

using System.Collections.Generic;
using System.IO.Abstractions.TestingHelpers;
using FakeItEasy;
using RepoM.Core.Plugin.Common;

internal static class MockFileSystemFactory
{
    public static MockFileSystem CreateDefaultFileSystem()
    {
        return new MockFileSystem(new Dictionary<string, MockFileData>()
            {
                { "C:\\tmp\\x.tmp", new MockFileData("x") }, // make sure path exists.
            });
    }

    public static IAppDataPathProvider CreateDefaultAppDataProvider()
    {
        IAppDataPathProvider appDataPathProvider = A.Fake<IAppDataPathProvider>();
        A.CallTo(() => appDataPathProvider.AppDataPath).Returns("C:\\tmp\\");
        return appDataPathProvider;
    }
}