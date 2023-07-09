namespace RepoM.Api.IO.Methods;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ExpressionStringEvaluator.Methods;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;

[UsedImplicitly]
public class FindFilesMethod : IMethod
{
    private readonly ILogger _logger;

    public FindFilesMethod(ILogger logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public bool CanHandle(string method)
    {
        return "FindFiles".Equals(method, StringComparison.CurrentCultureIgnoreCase);
    }

    public object? Handle(string method, params object?[] args)
    {
        if (args.Length < 2)
        {
            // not sure if we shouldn't throw.
            return null;
        }

        // first arg = root path
        if (args[0] is not string rootPath)
        {
            // not sure if we shouldn't throw.
            return null;
        }

        // second arg = *.ext
        if (args[1] is not string searchPattern)
        {
            // not sure if we shouldn't throw.
            return null;
        }

        try
        {
            return GetFileEnumerator(rootPath, searchPattern).ToArray();
        }
        catch (Exception e)
        {
            // not sure if we shouldn't throw.
            _logger.LogError(e, "Could nog find files according to path {rootPath} and searchPattern {searchPattern}. {message}", rootPath, searchPattern, e.Message);
            return null;
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
               .Where(f => !f.StartsWith('.'));
    }
}