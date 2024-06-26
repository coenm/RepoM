namespace RepoM.Plugin.WindowsExplorerGitInfo.PInvoke.Explorer;

using System;
using System.Collections;
using System.IO;

internal abstract class ExplorerWindowActor
{
    private Type? _shellApplicationType;

    public void Pulse()
    {
        _shellApplicationType ??= Type.GetTypeFromProgID("Shell.Application");

        if (_shellApplicationType == null)
        {
            return;
        }

        try
        {
            var comShellApplication = Activator.CreateInstance(_shellApplicationType);
            if (comShellApplication == null)
            {
                return;
            }

            using var shell = new ComBridge(comShellApplication);
            IEnumerable? comWindows = shell.InvokeMethod<IEnumerable>("Windows");
            if (comWindows == null)
            {
                return;
            }

            foreach (var comWindow in comWindows)
            {
                if (comWindow == null)
                {
                    continue;
                }

                using var window = new ComBridge(comWindow);
                var fullName = window.GetPropertyValue<string>("FullName");
                var executable = Path.GetFileName(fullName);

                if (string.Equals(executable, "explorer.exe", StringComparison.OrdinalIgnoreCase))
                {
                    // thanks http://docwiki.embarcadero.com/Libraries/Seattle/en/SHDocVw.IWebBrowser2_Properties
                    var hwnd = window.GetPropertyValue<long>("hwnd");
                    var locationUrl = window.GetPropertyValue<string>("LocationURL");

                    Act((IntPtr)hwnd, locationUrl);
                }
            }
        }
        catch (System.Runtime.InteropServices.COMException)
        {
            /* this is fire & forget - nothing we can do in here for unreproducible exceptions */
        }
        catch (System.Reflection.TargetInvocationException)
        {
            /* this is fire & forget - nothing we can do in here for unreproducible exceptions */
        }
    }

    protected abstract void Act(IntPtr hwnd, string? explorerLocationUrl);
}