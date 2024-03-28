namespace RepoM.ActionMenu.CodeGen.Misc;

using System;
using System.IO;

public static class FileSystemHelper
{
    public static void DeleteFileIsExist(string pathToGeneratedCode)
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

    public static void CheckDirectory(string path)
    {
        if (!Directory.Exists(path))
        {
            throw new Exception($"Folder '{path}' does not exist");
        }
    }

    public static void CheckFile(string path)
    {
        if (!File.Exists(path))
        {
            throw new Exception($"File '{path}' does not exist");
        }
    }
}