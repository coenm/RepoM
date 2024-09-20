namespace RepoM.ActionMenu.CodeGen.Misc;

using System;
using System.IO;

internal static class FileSystemHelper
{
    public static void DeleteFileIfExist(string pathToGeneratedCode)
    {
        if (!File.Exists(pathToGeneratedCode))
        {
            return;
        }

        try
        {
            File.Delete(pathToGeneratedCode);
        }
        catch (Exception e)
        {
            Console.WriteLine($"Could not delete generated file '{pathToGeneratedCode}'. {e.Message}");
            throw;
        }
    }

    public static void CheckDirectoryExists(string path)
    {
        if (!Directory.Exists(path))
        {
            throw new Exception($"Folder '{path}' does not exist");
        }
    }

    public static void CheckFileExists(string path)
    {
        if (!File.Exists(path))
        {
            throw new Exception($"File '{path}' does not exist");
        }
    }
}