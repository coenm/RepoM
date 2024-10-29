namespace UiTests.Utils;

using FlaUI.Core;

public static class ApplicationFactory
{
    public const string VS_CODE_EXE = @"C:\Users\Munckhof CJJ\AppData\Local\Programs\Microsoft VS Code\Code.exe";

    public static Application LaunchNewEmptyVsCode()
    {
        return Application.Launch(
            @"""C:\Users\Munckhof CJJ\AppData\Local\Programs\Microsoft VS Code\Code.exe""",
            "--new-window  --disable-extensions");
    }

    public static Application OpenFileInVsCode(string? filename)
    {
        if (string.IsNullOrWhiteSpace(filename))
        {
            filename = string.Empty;
        }
        else
        {
            filename = @" """ + filename + @"""";
        }

        return Application.Launch(
            @"""C:\Users\Munckhof CJJ\AppData\Local\Programs\Microsoft VS Code\Code.exe""",
            "--reuse-window " + filename);
    }



    public static Application LaunchVsCode(string? filename)
    {
        if (string.IsNullOrWhiteSpace(filename))
        {
            filename = string.Empty;
        }
        else
        {
            filename = @" """ + filename + @"""";
        }

        return Application.Launch(
            @"""C:\Users\Munckhof CJJ\AppData\Local\Programs\Microsoft VS Code\Code.exe""",
            "--new-window  --disable-extensions" + filename);
    }

    public static Application LaunchRepoM(string configPath)
    {
        while (configPath.EndsWith('\\'))
        {
            configPath = configPath.TrimEnd('\\');
        }

        return Application.Launch(
            @"C:\Projects\Private\git\RepoM\src\RepoM.App\bin\Release\net8.0-windows\RepoM.exe",
            @$"--App:AppSettingsPath ""{configPath}"" ");
    }
}