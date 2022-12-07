namespace RepoM.Plugin.Statistics.Tests.Ordering;

using System.IO;
using System.IO.Abstractions.TestingHelpers;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using EasyTestFile;
using EasyTestFileXunit;

internal static class MockFileDataFactory
{
    public static async Task AddEasyFile(
        this MockFileSystem fs,
        string filename,
        EasyTestFileSettings? settings = null, 
        [CallerFilePath] string sourceFile = "",
        [CallerMemberName] string method = "")
    {
        await using Stream stream = await EasyTestFile.LoadAsStream(settings, sourceFile, method);
        fs.AddFile(filename, new MockFileData(StreamToBytes(stream)));
    }

    private static byte[] StreamToBytes(Stream input)
    {
        if (input is MemoryStream ms)
        {
            return ms.ToArray();
        }

        using var ms1 = new MemoryStream();
        input.CopyTo(ms1);
        return ms1.ToArray();
    }
}