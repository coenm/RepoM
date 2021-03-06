namespace RepoM.Api.Git;

using System;
using System.Diagnostics;
using System.IO;
using System.Xml.Linq;

[DebuggerDisplay("{Key}/{Name}")]
public class Remote
{
    public Remote(string key, string url)
    {
        Key = key;
        Url = url;
        Name = CalculateName(url);
    }

    public string Key { get; }

    public string Name { get; set; }

    public string Url { get; }

    private static string CalculateName(string url)
    {
        string name;

        try
        {
            var fi = new FileInfo(url);
            name = fi.Name.Trim();
            name = Sanitize(name);
        }
        catch (Exception)
        {
            name = string.Empty;
        }

        if (!string.IsNullOrWhiteSpace(name))
        {
            return name;
        }
        
        try
        {
            var parts = url.Split('/', '\\');
            name = parts[parts.Length - 1];
            name = Sanitize(name);
        }
        catch (Exception)
        {
            name = string.Empty;
        }

        return name;
    }

    private static string Sanitize(string input)
    {
        var output = input;

        if (output.EndsWith(".git", StringComparison.CurrentCultureIgnoreCase))
        {
            output = output.Substring(0, output.Length - ".git".Length);
        }

        return output.Replace("%20", " ");
    }
}