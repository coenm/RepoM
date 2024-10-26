namespace UiTests.Utils;

using FlaUI.Core;

public static class ApplicationFactory
{
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

    public static Application LaunchRepoM()
    {
        return Application.Launch(@"C:\Projects\Private\git\RepoM\src\RepoM.App\bin\Release\net8.0-windows\RepoM.exe");
    }
}