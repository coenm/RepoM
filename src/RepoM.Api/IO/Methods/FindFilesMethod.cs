namespace RepoM.Api.IO.Methods;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ExpressionStringEvaluator.Methods;

public class FindFilesMethod : IMethod
{
    public bool CanHandle(string method)
    {
        return "FindFiles".Equals(method, StringComparison.CurrentCultureIgnoreCase);
    }

    public CombinedTypeContainer Handle(string method, params CombinedTypeContainer[] args)
    {
        if (args.Length < 2)
        {
            // not sure if we shouldn't throw.
            return CombinedTypeContainer.NullInstance;
        }

        // first arg = root path
        CombinedTypeContainer rootPathArg = args[0];
        if (!rootPathArg.IsString(out var rootPath))
        {
            // not sure if we shouldn't throw.
            return CombinedTypeContainer.NullInstance;
        }

        // second arg = *.ext
        CombinedTypeContainer searchPatternArg = args[1];
        if (!searchPatternArg.IsString(out var searchPattern))
        {
            // not sure if we shouldn't throw.
            return CombinedTypeContainer.NullInstance;
        }

        try
        {
            var files = GetFileEnumerator(rootPath, searchPattern).ToArray();
            return new CombinedTypeContainer(files.Select(f => new CombinedTypeContainer(f)).ToArray());
        }
        catch (Exception)
        {
            // not sure if we shouldn't throw.
            return CombinedTypeContainer.NullInstance;
        }
    }
    
    private static IEnumerable<string> GetFileEnumerator(string path, string searchPattern)
    {
        // prefer EnumerateFileSystemInfos() over EnumerateFiles() to include packaged folders like
        // .app or .xcodeproj on macOS

        var directory = new DirectoryInfo(path);
        return directory
               .EnumerateFileSystemInfos(searchPattern, SearchOption.AllDirectories)
               .Select(f => f.FullName)
               .Where(f => !f.StartsWith("."));
    }
}