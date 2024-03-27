namespace RepoM.Plugin.LuceneQueryParser;

using System;
using JetBrains.Annotations;

public sealed class ParseException : Exception
{
    public ParseException(string? message, string text, Exception? innerException) : base(message, innerException)
    {
        Text = text;
    }

    [PublicAPI]
    public string Text { get; }

    public override string ToString()
    {
        return $"{base.ToString()} : {Text}";
    }
}